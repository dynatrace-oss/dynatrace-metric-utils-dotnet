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

namespace Dynatrace.MetricUtils
{
	/// <summary>Metric is the intermittent data type that can then be serialized.</summary>
	public class Metric
	{
		/// <summary>Internal constructor to be used by the <c>MetricsFactory</c></summary>
		/// <param name="metricName">The name of the metric.</param>
		/// <param name="dimensions">A list of dimensions to be added to this metric</param>
		/// <param name="value">The metric value, one of the implementations in MetricValue.cs</param>
		/// <param name="timestamp">An optional timestamp.</param>
		internal Metric(string metricName, IEnumerable<KeyValuePair<string, string>> dimensions, IMetricValue value,
			DateTime? timestamp)
		{
			if (string.IsNullOrEmpty(metricName))
			{
				throw new MetricException("Metric name can't be null or empty.");
			}

			MetricName = metricName;
			Dimensions = dimensions;
			Value = value;
			Timestamp = timestamp;
		}

		/// <summary>The name of the metric.</summary>
		public string MetricName { get; }

		/// <summary>Dimensions for this metric.</summary>
		public IEnumerable<KeyValuePair<string, string>> Dimensions { get; }

		/// <summary>The metric vale in the form of a <see cref="IMetricValue" />.</summary>
		public IMetricValue Value { get; }

		/// <summary>An optional timestamp for the metric.</summary>
		/// <value></value>
		public DateTime? Timestamp { get; }
	}
}
