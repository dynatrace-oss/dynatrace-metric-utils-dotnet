// <copyright company="Dynatrace LLC">
// Copyright 2020 Dynatrace LLC
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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dynatrace.MetricUtils.Tests
{
	public class MetricSerializerTests
	{
		private static readonly ILogger<MetricSerializerTests> _logger = NullLogger<MetricSerializerTests>.Instance;

		private static readonly DateTime testTimestamp = DateTimeOffset.FromUnixTimeMilliseconds(1604660600000).UtcDateTime;

		[Fact]
		public void SerializeLongDeltaCounter()
		{
			var serializer = new MetricSerializer(_logger);

			var dimensions = new List<KeyValuePair<string, string>> {
				new KeyValuePair<string, string>("label1", "value1"),
				new KeyValuePair<string, string>("label2", "value2")
			};
			var metric = MetricsFactory.CreateLongDeltaCounter("metric1", 100, dimensions, testTimestamp);

			string serialized = serializer.SerializeMetric(metric);
			string expected = "metric1,label1=value1,label2=value2,dt.metrics.source=opentelemetry count,delta=100 1604660600000" + Environment.NewLine;
			Assert.Equal(expected, serialized);

			var metricWithCurrentTimestamp = MetricsFactory.CreateLongDeltaCounter("metric2", 200, dimensions);
			string serializedWithCurrentTimestamp = serializer.SerializeMetric(metricWithCurrentTimestamp);

			Assert.Equal(expected.Length, serializedWithCurrentTimestamp.Length);
			Assert.StartsWith("metric2,label1=value1,label2=value2,dt.metrics.source=opentelemetry count,delta=200 ", serializedWithCurrentTimestamp);
		}


		// [Fact]
		// public void TestDimensionValuesNormalized()
		// {
		//     var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1604660628881).UtcDateTime;

		//     var labels = new List<KeyValuePair<string, string>> {
		//         new KeyValuePair<string, string>("label1", "\\=\" =="),
		//     };
		//     var metric = new Metric("namespace1", "metric1", "Description", AggregationType.LongSum);
		//     metric.Data.Add(new Int64SumData
		//     {
		//         Labels = labels,
		//         Sum = 100,
		//         Timestamp = timestamp
		//     });

		//     string serialized = new DynatraceMetricSerializer(_logger).SerializeMetric(metric);
		//     string expected = "namespace1.metric1,label1=\\\\\\=\"\\ \\=\\=,dt.metrics.source=opentelemetry count,delta=100 1604660628881" + Environment.NewLine;
		//     Assert.Equal(expected, serialized);
		// }

		// [Fact]
		// public void SerializeWithoutLabels()
		// {
		//     var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1604660628881).UtcDateTime;

		//     var metric = new Metric("namespace1", "metric1", "Description", AggregationType.LongSum);
		//     metric.Data.Add(new Int64SumData
		//     {
		//         Labels = new List<KeyValuePair<string, string>>(),
		//         Sum = 100,
		//         Timestamp = timestamp
		//     });

		//     string serialized = new DynatraceMetricSerializer(_logger).SerializeMetric(metric);
		//     string expected = "namespace1.metric1,dt.metrics.source=opentelemetry count,delta=100 1604660628881" + Environment.NewLine;
		//     Assert.Equal(expected, serialized);
		// }

		// [Fact]
		// public void PrefixOption()
		// {
		//     var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1604660628881).UtcDateTime;

		//     var labels = new List<KeyValuePair<string, string>>();
		//     var metric = new Metric("namespace1", "metric1", "Description", AggregationType.LongSum);
		//     metric.Data.Add(new Int64SumData
		//     {
		//         Labels = labels,
		//         Sum = 100,
		//         Timestamp = timestamp
		//     });

		//     string serialized = new DynatraceMetricSerializer(_logger, prefix: "prefix1").SerializeMetric(metric);
		//     string expected = "prefix1.namespace1.metric1,dt.metrics.source=opentelemetry count,delta=100 1604660628881" + Environment.NewLine;
		//     Assert.Equal(expected, serialized);
		// }

		// [Fact]
		// public void PrefixOptionWithTrailingDot()
		// {
		//     var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1604660628881).UtcDateTime;

		//     var labels = new List<KeyValuePair<string, string>>();
		//     var metric = new Metric("namespace1", "metric1", "Description", AggregationType.LongSum);
		//     metric.Data.Add(new Int64SumData
		//     {
		//         Labels = labels,
		//         Sum = 100,
		//         Timestamp = timestamp
		//     });

		//     string serialized = new DynatraceMetricSerializer(_logger, prefix: "prefix.").SerializeMetric(metric);
		//     string expected = "prefix.namespace1.metric1,dt.metrics.source=opentelemetry count,delta=100 1604660628881" + Environment.NewLine;
		//     Assert.Equal(expected, serialized);
		// }

		// [Fact]
		// public void DimensionsOption()
		// {
		//     var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1604660628881).UtcDateTime;

		//     var labels = new List<KeyValuePair<string, string>> {
		//         new KeyValuePair<string, string>("label1", "value1"),
		//         new KeyValuePair<string, string>("label2", "value2")
		//     };
		//     var metric = new Metric("namespace1", "metric1", "Description", AggregationType.LongSum);
		//     metric.Data.Add(new Int64SumData
		//     {
		//         Labels = labels,
		//         Sum = 100,
		//         Timestamp = timestamp
		//     });

		//     string serialized = new DynatraceMetricSerializer(_logger,
		//     defaultDimensions: new List<KeyValuePair<string, string>> {
		//         new KeyValuePair<string, string>("default1", "value1") ,
		//         new KeyValuePair<string, string>("default2", "value2") ,
		//         new KeyValuePair<string, string>("default3", "value3") ,
		//     }).SerializeMetric(metric);
		//     string expected = "namespace1.metric1,default1=value1,default2=value2,default3=value3,label1=value1,label2=value2,dt.metrics.source=opentelemetry count,delta=100 1604660628881" + Environment.NewLine;
		//     Assert.Equal(expected, serialized);
		// }

		// [Fact]
		// public void SerializeLongSumBatch()
		// {
		//     var labels = new List<KeyValuePair<string, string>>{
		//         new KeyValuePair<string, string>("label1", "value1"),
		//         new KeyValuePair<string, string>("label2", "value2")
		//     };
		//     var metric = new Metric("namespace1", "metric1", "Description", AggregationType.LongSum);
		//     metric.Data.Add(new Int64SumData
		//     {
		//         Labels = labels,
		//         Sum = 100,
		//         Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1604660628881).UtcDateTime
		//     });
		//     metric.Data.Add(new Int64SumData
		//     {
		//         Labels = labels,
		//         Sum = 130,
		//         Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1604660628882).UtcDateTime
		//     });
		//     metric.Data.Add(new Int64SumData
		//     {
		//         Labels = labels,
		//         Sum = 150,
		//         Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1604660628883).UtcDateTime
		//     });

		//     string serialized = new DynatraceMetricSerializer(_logger).SerializeMetric(metric);
		//     string expected =
		//         "namespace1.metric1,label1=value1,label2=value2,dt.metrics.source=opentelemetry count,delta=100 1604660628881" + Environment.NewLine +
		//         "namespace1.metric1,label1=value1,label2=value2,dt.metrics.source=opentelemetry count,delta=130 1604660628882" + Environment.NewLine +
		//         "namespace1.metric1,label1=value1,label2=value2,dt.metrics.source=opentelemetry count,delta=150 1604660628883" + Environment.NewLine;
		//     Assert.Equal(expected, serialized);
		// }

		// [Fact]
		// public void DimensionPrecedence()
		// {
		//     var timestamp = DateTimeOffset.FromUnixTimeMilliseconds(1604660628881).UtcDateTime;

		//     var defaultDimensions = new Dictionary<string, string> { { "dimension1", "default" }, { "dimension2", "default" }, { "dimension3", "default" } };
		//     var labels = new List<KeyValuePair<string, string>> {
		//         new KeyValuePair<string, string>("dimension2", "label"),
		//         new KeyValuePair<string, string>("dimension3", "label")
		//     };
		//     var staticDimensions = new Dictionary<string, string> { { "dimension3", "static" } };

		//     var metric = new Metric("namespace1", "metric1", "Description", AggregationType.LongSum);
		//     metric.Data.Add(new Int64SumData
		//     {
		//         Labels = labels,
		//         Sum = 100,
		//         Timestamp = timestamp
		//     });

		//     var serialized = new DynatraceMetricSerializer(_logger, null, defaultDimensions, staticDimensions).SerializeMetric(metric);

		//     string expected = "namespace1.metric1,dimension1=default,dimension2=label,dimension3=static,dt.metrics.source=opentelemetry count,delta=100 1604660628881" + Environment.NewLine;
		//     Assert.Equal(expected, serialized);
		// }
	}
}
