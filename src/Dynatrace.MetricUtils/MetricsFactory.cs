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

namespace Dynatrace.MetricUtils
{
	public static class MetricsFactory
	{
		public static Metric CreateLongTotalCounter(string metricName, long value, IEnumerable<KeyValuePair<string, string>> dimensions = null, DateTime timestamp = default)
		{
			if (dimensions == null)
				dimensions = Enumerable.Empty<KeyValuePair<string, string>>();

			if (default(DateTime) == timestamp)
				timestamp = DateTime.Now;

			return new Metric(metricName, dimensions, new MetricValue.LongCounterValue(value, false), timestamp);
		}

		public static Metric CreateLongDeltaCounter(string metricName, long value, IEnumerable<KeyValuePair<string, string>> dimensions = null, DateTime timestamp = default)
		{
			if (dimensions == null)
				dimensions = Enumerable.Empty<KeyValuePair<string, string>>();

			if (default(DateTime) == timestamp)
				timestamp = DateTime.Now;

			return new Metric(metricName, dimensions, new MetricValue.LongCounterValue(value, true), timestamp);
		}

		public static Metric CreateLongGauge(string metricName, long value, IEnumerable<KeyValuePair<string, string>> dimensions = null, DateTime timestamp = default)
		{
			if (dimensions == null)
				dimensions = Enumerable.Empty<KeyValuePair<string, string>>();

			if (default(DateTime) == timestamp)
				timestamp = DateTime.Now;

			return new Metric(metricName, dimensions, new MetricValue.LongGaugeValue(value), timestamp);
		}

		public static Metric CreateLongSummary(string metricName, long min, long max, long sum, long count, IEnumerable<KeyValuePair<string, string>> dimensions = null, DateTime timestamp = default)
		{
			if (dimensions == null)
				dimensions = Enumerable.Empty<KeyValuePair<string, string>>();

			if (default(DateTime) == timestamp)
				timestamp = DateTime.Now;

			return new Metric(metricName, dimensions, new MetricValue.LongSummaryValue(min, max, sum, count), timestamp);
		}

		public static Metric CreateDoubleTotalCounter(string metricName, double value, IEnumerable<KeyValuePair<string, string>> dimensions = null, DateTime timestamp = default)
		{
			if (dimensions == null)
				dimensions = Enumerable.Empty<KeyValuePair<string, string>>();

			if (default(DateTime) == timestamp)
				timestamp = DateTime.Now;

			return new Metric(metricName, dimensions, new MetricValue.DoubleCounterValue(value, false), timestamp);
		}

		public static Metric CreateDoubleDeltaCounter(string metricName, double value, IEnumerable<KeyValuePair<string, string>> dimensions = null, DateTime timestamp = default)
		{
			if (dimensions == null)
				dimensions = Enumerable.Empty<KeyValuePair<string, string>>();

			if (default(DateTime) == timestamp)
				timestamp = DateTime.Now;

			return new Metric(metricName, dimensions, new MetricValue.DoubleCounterValue(value, true), timestamp);
		}

		public static Metric CreateDoubleGauge(string metricName, double value, IEnumerable<KeyValuePair<string, string>> dimensions = null, DateTime timestamp = default)
		{
			if (dimensions == null)
				dimensions = Enumerable.Empty<KeyValuePair<string, string>>();

			if (default(DateTime) == timestamp)
				timestamp = DateTime.Now;

			return new Metric(metricName, dimensions, new MetricValue.DoubleGaugeValue(value), timestamp);
		}

		public static Metric CreateDoubleSummary(string metricName, double min, double max, double sum, long count, IEnumerable<KeyValuePair<string, string>> dimensions = null, DateTime timestamp = default)
		{
			if (dimensions == null)
				dimensions = Enumerable.Empty<KeyValuePair<string, string>>();

			if (default(DateTime) == timestamp)
				timestamp = DateTime.Now;

			return new Metric(metricName, dimensions, new MetricValue.DoubleSummaryValue(min, max, sum, count), timestamp);
		}
	}
}
