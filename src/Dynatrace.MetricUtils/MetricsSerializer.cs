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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Dynatrace.MetricUtils
{
	public class MetricsSerializer
	{
		private const int MetricLineMaxLength = 2000;
		private static readonly int MaxDimensions = 50;
		private readonly List<KeyValuePair<string, string>> _defaultDimensions;
		private readonly ILogger _logger;
		private readonly string _prefix;
		private readonly List<KeyValuePair<string, string>> _staticDimensions;

		// public constructor.
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
			this._logger = logger ?? new NullLogger<MetricsSerializer>();
			this._prefix = prefix;
			this._defaultDimensions =
				Normalize.DimensionList(defaultDimensions) ?? new List<KeyValuePair<string, string>>();
			this._staticDimensions =
				Normalize.DimensionList(staticDimensions) ?? new List<KeyValuePair<string, string>>();
		}

		// this is required to read the Dynatrace metadata dimensions and still use constructor chaining
		private static List<KeyValuePair<string, string>> PrepareStaticDimensions(ILogger logger,
			bool enrichWithDynatraceMetadata, string metricsSource)
		{
			var staticDimensions = new List<KeyValuePair<string, string>>();

			if (enrichWithDynatraceMetadata)
			{
				var enricher = new OneAgentMetadataEnricher(logger);
				enricher.EnrichWithDynatraceMetadata(staticDimensions);
			}

			if (!string.IsNullOrEmpty(metricsSource))
			{
				staticDimensions.Add(new KeyValuePair<string, string>("dt.metrics.source", metricsSource));
			}

			return staticDimensions;
		}

		public string SerializeMetric(Metric metric)
		{
			var sb = new StringBuilder();
			this.SerializeMetric(sb, metric);
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
			var metricKey = this.CreateMetricKey(metric);
			// skip lines with invalid metric keys.
			if (string.IsNullOrEmpty(metricKey))
			{
				throw new MetricException("Metric key can't be undefined.");
			}

			sb.Append(metricKey);

			// default dimensions and static dimensions are normalized once upon serializer creation.
			// the labels from opentelemetry are normalized here, then all dimensions are merged.
			var normalizedDimensions = MergeDimensions(this._defaultDimensions,
				Normalize.DimensionList(metric.Dimensions), this._staticDimensions);

			// merged dimensions are normalized and escaped since we called Normalize.DimensionList on each of the sublists.
			this.WriteDimensions(sb, normalizedDimensions);
			sb.Append($" {metric.Value.Serialize()}");

			this.WriteTimestamp(sb, metric.Timestamp);
		}

		private void WriteTimestamp(StringBuilder sb, DateTime timestamp)
		{
			sb.Append($" {new DateTimeOffset(timestamp.ToLocalTime()).ToUnixTimeMilliseconds()}");
		}

		/// <returns>a valid metric key or null, if the input could not be normalized</returns>
		private string CreateMetricKey(Metric metric)
		{
			var keyBuilder = new StringBuilder();
			if (!string.IsNullOrEmpty(this._prefix))
			{
				keyBuilder.Append($"{this._prefix}.");
			}

			keyBuilder.Append(metric.MetricName);
			return Normalize.MetricKey(keyBuilder.ToString());
		}

		// Items from Enumerables passed further right overwrite items from Enumerables passed further left.
		// Pass only normalized dimension lists to this function.
		internal static List<KeyValuePair<string, string>> MergeDimensions(
			params IEnumerable<KeyValuePair<string, string>>[] dimensionLists)
		{
			var dictionary = new Dictionary<string, string>();

			if (dimensionLists == null)
			{
				return new List<KeyValuePair<string, string>>();
			}

			foreach (var dimensionList in dimensionLists)
			{
				if (dimensionList != null)
				{
					foreach (var dimension in dimensionList)
					{
						if (!dictionary.ContainsKey(dimension.Key))
						{
							dictionary.Add(dimension.Key, dimension.Value);
						}
						else
						{
							dictionary[dimension.Key] = dimension.Value;
						}
					}
				}
			}

			return dictionary.ToList();
		}

		// pass only normalized lists to this function.
		private void WriteDimensions(StringBuilder sb, List<KeyValuePair<string, string>> dimensions)
		{
			// should be negative if there are fewer dimensions than the maximum
			var diffToMaxDimensions = MaxDimensions - dimensions.Count;
			var toSkip = diffToMaxDimensions < 0 ? Math.Abs(diffToMaxDimensions) : 0;

			// if there are more dimensions, skip the first n dimensions so that 50 dimensions remain
			foreach (var dimension in dimensions.Skip(toSkip))
			{
				sb.Append($",{dimension.Key}={dimension.Value}");
			}
		}
	}
}
