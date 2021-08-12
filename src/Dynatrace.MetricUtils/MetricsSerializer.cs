// <copyright company="Dynatrace LLC">
// Copyright 2021 Dynatrace LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dynatrace.MetricUtils
{
	/// <summary>
	///     The MetricsSerializer can be used to transform
	///     <see cref="Metric" /> objects into strings that can be exported to a Dynatrace endpoint.
	/// </summary>
	public class MetricsSerializer
	{
		/// <summary>
		///     The maximum length of a line. The
		///     <see cref="MetricsSerializer.SerializeMetric(Metric)" /> method will throw a <see cref="MetricException" /> if this
		///     threshold is exceeded.
		/// </summary>
		private const int MetricLineMaxLength = 2000;

		/// <summary>
		///     If the timestamp describes a date before the year 2000 or after the year 3000, an error will be logged. Only one
		///     out of every <see cref="TimestampWarningThrottleFactor" /> error message will be printed.
		/// </summary>
		private const int TimestampWarningThrottleFactor = 1000;

		/// <summary>
		///     The atomic integer used to track how many error messages should have been printed.
		/// </summary>
		private static int _timestampWarningCounter;

		private readonly List<KeyValuePair<string, string>> _defaultDimensions;

		private readonly ILogger _logger;

		private readonly string _prefix;

		/// <summary>
		///     Static dimensions, e.g. Dynatrace metadata dimensions, and the <c>dt.metrics.source</c> dimension, which will be
		///     added to all metric lines created with this <see cref="MetricsSerializer" />.
		/// </summary>
		private readonly List<KeyValuePair<string, string>> _staticDimensions;

		/// <summary>
		///     Create a new <see cref="MetricsSerializer" />.
		/// </summary>
		/// <param name="logger">
		///     The logger to which messages should be exported. If nothing is set, no log messages will be
		///     printed.
		/// </param>
		/// <param name="prefix">The prefix to add to all metrics serialized with this Serializer.</param>
		/// <param name="defaultDimensions">Default dimensions that are added to all metrics seraialized with this Serializer.</param>
		/// <param name="metricsSource">
		///     If set, a dimension named <c>dt.metrics.source</c> with the value of this parameter will be
		///     added to all metric lines created by this Serializer.
		/// </param>
		/// <param name="enrichWithDynatraceMetadata">
		///     Read Dynatrace metadata and add it to all metric lines created. On
		///     (<c>true</c>) by default.
		/// </param>
		public MetricsSerializer(ILogger logger = null, string prefix = null,
			IEnumerable<KeyValuePair<string, string>> defaultDimensions = null,
			string metricsSource = null, bool enrichWithDynatraceMetadata = true)
			: this(logger, prefix, defaultDimensions,
				PrepareStaticDimensions(logger, enrichWithDynatraceMetadata, metricsSource))
		{
		}

		// internal constructor offers an interface for testing and is used by the public constructor
		internal MetricsSerializer(ILogger logger, string prefix,
			IEnumerable<KeyValuePair<string, string>> defaultDimensions,
			List<KeyValuePair<string, string>> staticDimensions)
		{
			_logger = logger == null ? NullLogger.Instance : logger;
			_prefix = null == prefix ? "" : prefix;
			_defaultDimensions = Normalize.DimensionList(defaultDimensions) ?? new List<KeyValuePair<string, string>>();
			_staticDimensions = Normalize.DimensionList(staticDimensions) ?? new List<KeyValuePair<string, string>>();
		}

		// this is required to read the Dynatrace metadata dimensions and still use constructor chaining
		private static List<KeyValuePair<string, string>> PrepareStaticDimensions(ILogger logger,
			bool enrichWithDynatraceMetadata, string metricsSource)
		{
			var staticDimensions = new List<KeyValuePair<string, string>>();

			if (enrichWithDynatraceMetadata)
			{
				var enricher = new DynatraceMetadataEnricher(logger == null ? NullLogger.Instance : logger, new DefaultFileReader());
				enricher.EnrichWithDynatraceMetadata(staticDimensions);
			}

			if (!string.IsNullOrEmpty(metricsSource))
			{
				staticDimensions.Add(new KeyValuePair<string, string>("dt.metrics.source", metricsSource));
			}

			return staticDimensions;
		}

		/// <summary>
		///     Create a <see cref="string" /> from a metric.
		///     Default dimensions of the serializer, dimensions on the metric, Dynatrace metadata dimensions, and
		///     "dt.metrics.source" dimensions are merged and added to the metric line.
		///     Will throw a <see cref="MetricException" /> if the metric name is invalid, or if the serialized line exceeds the
		///     line length threshold.
		/// </summary>
		/// <param name="metric"></param>
		/// <returns></returns>
		public string SerializeMetric(Metric metric)
		{
			var sb = new StringBuilder();
			SerializeMetric(sb, metric);
			var serialized = sb.ToString();
			if (serialized.Length > MetricLineMaxLength)
			{
				throw new MetricException(
					$"Metric line exceeds line length of {MetricLineMaxLength} characters (Metric name: '{metric.MetricName}').");
			}

			return serialized;
		}

		private void SerializeMetric(StringBuilder sb, Metric metric)
		{
			var metricKey = CreateMetricKey(metric.MetricName);
			// throw on lines with invalid metric keys.
			if (string.IsNullOrEmpty(metricKey))
			{
				throw new MetricException("Metric name can't be null or empty.");
			}

			sb.Append(metricKey);

			// default dimensions and static dimensions are normalized once upon serializer creation.
			// the labels from opentelemetry are normalized here, then all dimensions are merged.
			var normalizedDimensions = MergeDimensions(_defaultDimensions,
				Normalize.DimensionList(metric.Dimensions), _staticDimensions);

			// merged dimensions are normalized and escaped since we called Normalize.DimensionList on each of the sublists.
			WriteDimensions(sb, normalizedDimensions);
			sb.Append($" {metric.Value.Serialize()}");

			if (metric.Timestamp.HasValue)
			{
				WriteTimestamp(sb, metric.Timestamp.Value);
			}
		}

		/// <summary>
		///     Add the timestamp to the builder.
		/// </summary>
		private void WriteTimestamp(StringBuilder sb, DateTime timestamp)
		{
			if (timestamp.Year < 2000 || timestamp.Year > 3000)
			{
				if (Interlocked.Increment(ref _timestampWarningCounter) == 1)
				{
					var time = timestamp.ToString(CultureInfo.InvariantCulture);
					_logger.LogWarning(
						$"Order of magnitude of the timestamp seems off ({time}). "
						+ "The timestamp represents a time before the year 2000 or after the year 3000. "
						+ "Skipping setting timestamp, the current server time will be added upon ingestion. "
						+ $"Only one out of every {TimestampWarningThrottleFactor} of these messages will be printed.");
				}

				Interlocked.CompareExchange(ref _timestampWarningCounter, 0, TimestampWarningThrottleFactor);
				return;
			}


			sb.Append($" {new DateTimeOffset(timestamp.ToLocalTime()).ToUnixTimeMilliseconds()}");
		}

		/// <summary>
		///     Create the metric key by prefixing a prefix if it exists and normalizing the key.
		/// </summary>
		/// <param name="metricName">The metric name to normalize.</param>
		/// <returns>The complete, prefixed and normalized metric key.</returns>
		private string CreateMetricKey(string metricName)
		{
			var keyBuilder =
				new StringBuilder(_prefix.Length + metricName.Length + 1);
			if (!string.IsNullOrEmpty(_prefix))
			{
				keyBuilder.Append($"{_prefix}.");
			}

			keyBuilder.Append(metricName);
			return Normalize.MetricKey(keyBuilder.ToString());
		}

		/// <summary>
		///     Items from Enumerables passed further right overwrite items from Enumerables passed further left.
		///     Pass only normalized dimension lists to this function.
		///     The order of the items in the lists will be preserved.
		/// </summary>
		/// <param name="dimensionLists">One or more dimension lists to merge. Pass only normalized lists to this function.</param>
		/// <returns>A dimension list containing the merged lists. No duplicate keys will exist in this list.</returns>
		internal static List<KeyValuePair<string, string>> MergeDimensions(
			params IEnumerable<KeyValuePair<string, string>>[] dimensionLists)
		{
			var dictionary = new OrderedDictionary();

			if (dimensionLists == null)
			{
				return new List<KeyValuePair<string, string>>();
			}

			foreach (var dimensionList in dimensionLists)
			{
				if (dimensionList == null)
				{
					continue;
				}

				foreach (var dimension in dimensionList)
				{
					if (!dictionary.Contains(dimension.Key))
					{
						dictionary.Add(dimension.Key, dimension.Value);
					}
					else
					{
						dictionary[dimension.Key] = dimension.Value;
					}
				}
			}

			var outList = new List<KeyValuePair<string, string>>(dictionary.Count);

			foreach (DictionaryEntry entry in dictionary)
			{
				outList.Add(new KeyValuePair<string, string>(entry.Key.ToString(), entry.Value.ToString()));
			}

			return outList;
		}

		/// <summary>
		///     Serialize and add the dimensions to the builder. Pass only normalized dimensions to this method.
		///     If there are more than the maximum number of dimensions, skips dimensions until only the maximum dimensions remains
		///     and serializes those.
		/// </summary>
		private void WriteDimensions(StringBuilder sb, List<KeyValuePair<string, string>> dimensions)
		{
			// should be negative if there are fewer dimensions than the maximum
			var diffToMaxDimensions = DynatraceMetricApiConstants.MaximumDimensions - dimensions.Count;
			var toSkip = diffToMaxDimensions < 0 ? Math.Abs(diffToMaxDimensions) : 0;

			// if there are more dimensions, skip the first n dimensions so that 50 dimensions remain
			foreach (var dimension in dimensions.Skip(toSkip))
			{
				sb.Append($",{dimension.Key}={dimension.Value}");
			}
		}
	}
}
