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
	/// <summary>Static class with methods to create <see cref="DynatraceMetric" /> objects.</summary>
	public static class DynatraceMetricsFactory
	{
		/// <summary>
		/// Creates a counter metric for an integer. The value will be serialized as "count,delta=[value]". Only a delta
		/// to the previously exported value can be specified here.
		/// </summary>
		/// <param name="metricName">The name of the metric.</param>
		/// <param name="value">The value to be set on the metric.</param>
		/// <param name="dimensions">A list of key-value pairs to set on this metric.</param>
		/// <param name="timestamp">The timestamp at which the metric was recorded.</param>
		/// <returns>A <see cref="DynatraceMetric" /> object </returns>
		public static DynatraceMetric CreateLongCounterDelta(string metricName, long value,
			IEnumerable<KeyValuePair<string, string>> dimensions = null, DateTimeOffset? timestamp = null) =>
			new DynatraceMetric(metricName, dimensions, new MetricValue.LongCounterValue(value), timestamp);

		/// <summary>Creates a gauge metric for an integer. The value will be serialized as "gauge,[value]".</summary>
		/// <param name="metricName">The name of the metric.</param>
		/// <param name="value">The value to be set on the metric.</param>
		/// <param name="dimensions">A list of key-value pairs to set on this metric.</param>
		/// <param name="timestamp">The timestamp at which the metric was recorded.</param>
		/// <returns>A <see cref="DynatraceMetric" /> object </returns>
		public static DynatraceMetric CreateLongGauge(string metricName, long value,
			IEnumerable<KeyValuePair<string, string>> dimensions = null, DateTimeOffset? timestamp = null) =>
			new DynatraceMetric(metricName, dimensions, new MetricValue.LongGaugeValue(value), timestamp);

		/// <summary>
		/// Creates a summary metric for integers. The value will be serialized as
		/// "gauge,min=[min],max=[max],sum=[sum],count=[count]".
		/// </summary>
		/// <param name="metricName">The name of the metric.</param>
		/// <param name="min">The smallest value in the summary</param>
		/// <param name="max">The largest value in the summary</param>
		/// <param name="sum">The sum of all values in the summary.</param>
		/// <param name="count">The number of observations combined in the summary.</param>
		/// <param name="dimensions">A list of key-value pairs to set on this metric.</param>
		/// <param name="timestamp">The timestamp at which the metric was recorded.</param>
		/// <exception cref="DynatraceMetricException">Thrown when the count is lower than 0 or if min is larger than max.</exception>
		/// <returns>A <see cref="DynatraceMetric" /> object </returns>
		public static DynatraceMetric CreateLongSummary(string metricName, long min, long max, long sum, long count,
			IEnumerable<KeyValuePair<string, string>> dimensions = null, DateTimeOffset? timestamp = null) =>
			new DynatraceMetric(metricName, dimensions, new MetricValue.LongSummaryValue(min, max, sum, count),
				timestamp);

		/// <summary>
		/// Creates a counter metric for a floating point number. The value will be serialized as "count,delta=[value]".
		/// Only a delta to the previously exported value can be specified here.
		/// </summary>
		/// <param name="metricName">The name of the metric.</param>
		/// <param name="value">The value to be set on the metric.</param>
		/// <param name="dimensions">A list of key-value pairs to set on this metric.</param>
		/// <param name="timestamp">The timestamp at which the metric was recorded.</param>
		/// <exception cref="DynatraceMetricException">Thrown if the value is Infinite or NaN.</exception>
		/// <returns>A <see cref="DynatraceMetric" /> object </returns>
		public static DynatraceMetric CreateDoubleCounterDelta(string metricName, double value,
			IEnumerable<KeyValuePair<string, string>> dimensions = null, DateTimeOffset? timestamp = null) =>
			new DynatraceMetric(metricName, dimensions, new MetricValue.DoubleCounterValue(value), timestamp);

		/// <summary>Creates a gauge metric for a floating point number. The value will be serialized as "gauge,[value]".</summary>
		/// <param name="metricName">The name of the metric.</param>
		/// <param name="value">The value to be set on the metric.</param>
		/// <param name="dimensions">A list of key-value pairs to set on this metric.</param>
		/// <param name="timestamp">The timestamp at which the metric was recorded.</param>
		/// <exception cref="DynatraceMetricException">Thrown if the value is Infinite or NaN.</exception>
		/// <returns>A <see cref="DynatraceMetric" /> object </returns>
		public static DynatraceMetric CreateDoubleGauge(string metricName, double value,
			IEnumerable<KeyValuePair<string, string>> dimensions = null, DateTimeOffset? timestamp = null) =>
			new DynatraceMetric(metricName, dimensions, new MetricValue.DoubleGaugeValue(value), timestamp);

		/// <summary>
		/// Creates a summary metric for floating point numbers. The value will be serialized as
		/// "gauge,min=[min],max=[max],sum=[sum],count=[count]".
		/// </summary>
		/// <param name="metricName">The name of the metric.</param>
		/// <param name="min">The smallest value in the summary</param>
		/// <param name="max">The largest value in the summary</param>
		/// <param name="sum">The sum of all values in the summary.</param>
		/// <param name="count">The number of observations combined in the summary.</param>
		/// <param name="dimensions">A list of key-value pairs to set on this metric.</param>
		/// <param name="timestamp">The timestamp at which the metric was recorded.</param>
		/// <exception cref="DynatraceMetricException">
		/// Thrown Thrown when the count is lower than 0 or if min is larger than max or if any
		/// of the values is Infinite or NaN.
		/// </exception>
		/// <returns>A <see cref="DynatraceMetric" /> object </returns>
		public static DynatraceMetric CreateDoubleSummary(string metricName, double min, double max, double sum, long count,
			IEnumerable<KeyValuePair<string, string>> dimensions = null, DateTimeOffset? timestamp = null) =>
			new DynatraceMetric(metricName, dimensions, new MetricValue.DoubleSummaryValue(min, max, sum, count),
				timestamp);
	}
}
