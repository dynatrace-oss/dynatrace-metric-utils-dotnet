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
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Dynatrace.MetricUtils.Tests
{
	public class DynatraceMetricsSerializerTests
	{
		private static readonly ILogger<DynatraceMetricsSerializerTests> Logger = NullLogger<DynatraceMetricsSerializerTests>.Instance;

		// use the same timestamp for all tests
		private static readonly DateTimeOffset TestDatetime = DateTimeOffset.UtcNow;

		// string representation of the above DateTimeOffset in Unix milliseconds
		private static readonly string TestTimestamp =
			$"{TestDatetime.ToUnixTimeMilliseconds()}";

		private static readonly IEnumerable<KeyValuePair<string, string>> TestDimensions =
			new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("dim1", "value1"),
				new KeyValuePair<string, string>("dim2", "value2")
			};

		[Fact]
		public void SerializeLongCounter()
		{
			var serializer = new DynatraceMetricsSerializer(Logger);

			var serializedWithAllParams =
				serializer.SerializeMetric(
					DynatraceMetricsFactory.CreateLongCounterDelta("metric1", 100, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 count,delta=100 " + TestTimestamp);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(DynatraceMetricsFactory.CreateLongCounterDelta("metric2", 200, TestDimensions));
			serializedWithCurrentTimestamp.Should().Be("metric2,dim1=value1,dim2=value2 count,delta=200");

			var serializedWithMinimalParams =
				serializer.SerializeMetric(DynatraceMetricsFactory.CreateLongCounterDelta("metric3", 300));
			serializedWithMinimalParams.Should().Be("metric3 count,delta=300");
		}

		[Fact]
		public void SerializeLongGauge()
		{
			var serializer = new DynatraceMetricsSerializer(Logger);

			var serializedWithAllParams =
				serializer.SerializeMetric(
					DynatraceMetricsFactory.CreateLongGauge("metric1", 100, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 gauge,100 " + TestTimestamp);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(DynatraceMetricsFactory.CreateLongGauge("metric2", 200, TestDimensions));
			serializedWithCurrentTimestamp.Should().Be("metric2,dim1=value1,dim2=value2 gauge,200");

			var serializedWithMinimalParams =
				serializer.SerializeMetric(DynatraceMetricsFactory.CreateLongGauge("metric3", 300));
			serializedWithMinimalParams.Should().Be("metric3 gauge,300");
		}

		[Fact]
		public void SerializeLongSummary()
		{
			var serializer = new DynatraceMetricsSerializer(Logger);
			var serializedWithAllParams =
				serializer.SerializeMetric(
					DynatraceMetricsFactory.CreateLongSummary("metric1", 1, 3, 7, 4, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 gauge,min=1,max=3,sum=7,count=4 " + TestTimestamp);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(DynatraceMetricsFactory.CreateLongSummary("metric2", 1, 3, 7, 4, TestDimensions));
			serializedWithCurrentTimestamp.Should()
				.Be("metric2,dim1=value1,dim2=value2 gauge,min=1,max=3,sum=7,count=4");

			var serializedWithMinimalParams =
				serializer.SerializeMetric(DynatraceMetricsFactory.CreateLongSummary("metric3", 1, 3, 7, 4));
			serializedWithMinimalParams.Should().Be("metric3 gauge,min=1,max=3,sum=7,count=4");
		}

		[Fact]
		public void SerializeDoubleCounter()
		{
			var serializer = new DynatraceMetricsSerializer(Logger);

			var serializedWithAllParams =
				serializer.SerializeMetric(
					DynatraceMetricsFactory.CreateDoubleCounterDelta("metric1", 123.456, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 count,delta=123.456 " + TestTimestamp);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(DynatraceMetricsFactory.CreateDoubleCounterDelta("metric2", 223.456, TestDimensions));
			serializedWithCurrentTimestamp.Should().Be("metric2,dim1=value1,dim2=value2 count,delta=223.456");

			var serializedWithMinimalParams =
				serializer.SerializeMetric(DynatraceMetricsFactory.CreateDoubleCounterDelta("metric3", 323.456));
			serializedWithMinimalParams.Should().Be("metric3 count,delta=323.456");
		}

		[Fact]
		public void SerializeDoubleGauge()
		{
			var serializer = new DynatraceMetricsSerializer(Logger);

			var serializedWithAllParams =
				serializer.SerializeMetric(
					DynatraceMetricsFactory.CreateDoubleGauge("metric1", 123.456, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 gauge,123.456 " + TestTimestamp);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(DynatraceMetricsFactory.CreateDoubleGauge("metric2", 223.456, TestDimensions));
			serializedWithCurrentTimestamp.Should().Be("metric2,dim1=value1,dim2=value2 gauge,223.456");

			var serializedWithMinimalParams =
				serializer.SerializeMetric(DynatraceMetricsFactory.CreateDoubleGauge("metric3", 323.456));
			serializedWithMinimalParams.Should().Be("metric3 gauge,323.456");
		}

		[Fact]
		public void SerializeDoubleSummary()
		{
			var serializer = new DynatraceMetricsSerializer(Logger);
			var serializedWithAllParams =
				serializer.SerializeMetric(
					DynatraceMetricsFactory.CreateDoubleSummary("metric1", 1.2, 3.4, 7.8, 4, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 gauge,min=1.2,max=3.4,sum=7.8,count=4 " + TestTimestamp);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(
					DynatraceMetricsFactory.CreateDoubleSummary("metric2", 1.2, 3.4, 7.8, 4, TestDimensions));
			serializedWithCurrentTimestamp.Should()
				.Be("metric2,dim1=value1,dim2=value2 gauge,min=1.2,max=3.4,sum=7.8,count=4");

			var serializedWithMinimalParams =
				serializer.SerializeMetric(DynatraceMetricsFactory.CreateDoubleSummary("metric3", 1.2, 3.4, 7.8, 4));
			serializedWithMinimalParams.Should().Be("metric3 gauge,min=1.2,max=3.4,sum=7.8,count=4");
		}

		[Fact]
		public void TestDimensionValuesNormalized()
		{
			var dims = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("dim1", "\\=\" ==") };
			var metric = DynatraceMetricsFactory.CreateLongCounterDelta("metric1", 100, dims, TestDatetime);

			var serialized = new DynatraceMetricsSerializer(Logger).SerializeMetric(metric);
			serialized.Should()
				.Be("metric1,dim1=\\\\\\=\\\"\\ \\=\\= count,delta=100 " + TestTimestamp);
		}

		[Fact]
		public void TestPrefix()
		{
			var metric = DynatraceMetricsFactory.CreateLongCounterDelta("metric", 100, timestamp: TestDatetime);

			var serialized = new DynatraceMetricsSerializer(Logger, "prefix").SerializeMetric(metric);
			serialized.Should()
				.Be("prefix.metric count,delta=100 " + TestTimestamp);
		}

		[Fact]
		public void TestPrefixWithTrailingDot()
		{
			var metric = DynatraceMetricsFactory.CreateLongCounterDelta("metric", 100, timestamp: TestDatetime);
			var serializedWithTrailingPrefixDot = new DynatraceMetricsSerializer(Logger, "prefix.").SerializeMetric(metric);
			serializedWithTrailingPrefixDot.Should()
				.Be("prefix.metric count,delta=100 " + TestTimestamp);
		}

		[Fact]
		public void TestWithDefaultDimensions()
		{
			var metric = DynatraceMetricsFactory.CreateLongCounterDelta("metric", 100, TestDimensions, TestDatetime);
			var defaultDimensions = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("default1", "value1"),
				new KeyValuePair<string, string>("default2", "value2")
			};

			var serialized =
				new DynatraceMetricsSerializer(Logger, defaultDimensions: defaultDimensions).SerializeMetric(metric);
			serialized.Should()
				.Be("metric,default1=value1,default2=value2,dim1=value1,dim2=value2 count,delta=100 " + TestTimestamp);
		}

		[Fact]
		public void TestMetricsSource()
		{
			var metric = DynatraceMetricsFactory.CreateLongCounterDelta("metric", 100, null, TestDatetime);

			// the use case probably used most often
			new DynatraceMetricsSerializer(Logger, metricsSource: "opentelemetry").SerializeMetric(metric)
				.Should().Be("metric,dt.metrics.source=opentelemetry count,delta=100 " + TestTimestamp);

			// empty source will not be added
			new DynatraceMetricsSerializer(Logger, metricsSource: "").SerializeMetric(metric)
				.Should().Be("metric count,delta=100 " + TestTimestamp);

			// invalid characters in source will be escaped:
			new DynatraceMetricsSerializer(Logger, metricsSource: "esc\\ape=this\"").SerializeMetric(metric)
				.Should().Be("metric,dt.metrics.source=esc\\\\ape\\=this\\\" count,delta=100 " + TestTimestamp);
		}

		[Fact]
		public void TestDimensionPrecedence()
		{
			var defaultDimensions = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("dim1", "default1"),
				new KeyValuePair<string, string>("dim2", "default2"),
				new KeyValuePair<string, string>("dim3", "default3")
			};

			var metricDimensions =
				new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("dim2", "metric2"),
					new KeyValuePair<string, string>("dim3", "metric3")
				};

			var staticDimensions =
				new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("dim3", "static3") };

			var metric = DynatraceMetricsFactory.CreateLongCounterDelta("metric", 100, metricDimensions, TestDatetime);

			// using the internal constructor that accepts the static dimensions for testing:
			var serializer = new DynatraceMetricsSerializer(Logger, "prefix", defaultDimensions, staticDimensions);

			serializer.SerializeMetric(metric)
				.Should().Be("prefix.metric,dim1=default1,dim2=metric2,dim3=static3 count,delta=100 " + TestTimestamp);
		}

		[Fact]
		public void TestMergeDimensionsLeavesOneListAsIs()
		{
			var defaultDimensions = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("dim1", "default1"),
				new KeyValuePair<string, string>("dim2", "default2"),
				new KeyValuePair<string, string>("dim3", "default3")
			};

			DynatraceMetricsSerializer.MergeDimensions(defaultDimensions).Should().Equal(defaultDimensions);
		}

		[Fact]
		public void TestMergeDimensionsOverwritesInCorrectOrder()
		{
			var defaultDimensions = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("dim1", "default1"),
				new KeyValuePair<string, string>("dim2", "default2"),
				new KeyValuePair<string, string>("dim3", "default3")
			};

			var metricDimensions =
				new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("dim2", "metric2"),
					new KeyValuePair<string, string>("dim3", "metric3")
				};

			var staticDimensions =
				new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("dim3", "static3") };

			DynatraceMetricsSerializer.MergeDimensions(defaultDimensions, metricDimensions, staticDimensions).Should()
				.Equal(new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("dim1", "default1"),
					new KeyValuePair<string, string>("dim2", "metric2"),
					new KeyValuePair<string, string>("dim3", "static3")
				});
		}

		[Fact]
		public void TestMergeDimensionDisjunctKeys()
		{
			var someDimensions =
				new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("some1", "val1"),
					new KeyValuePair<string, string>("some2", "val2"),
					new KeyValuePair<string, string>("some3", "val3")
				};
			var otherDimensions = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("other1", "val1"),
				new KeyValuePair<string, string>("other2", "val2"),
				new KeyValuePair<string, string>("other3", "val3")
			};

			DynatraceMetricsSerializer.MergeDimensions(someDimensions, otherDimensions)
				.Should().Equal(
					new List<KeyValuePair<string, string>>
					{
						new KeyValuePair<string, string>("some1", "val1"),
						new KeyValuePair<string, string>("some2", "val2"),
						new KeyValuePair<string, string>("some3", "val3"),
						new KeyValuePair<string, string>("other1", "val1"),
						new KeyValuePair<string, string>("other2", "val2"),
						new KeyValuePair<string, string>("other3", "val3")
					}
				);
		}

		[Fact]
		public void TestMetricKeyInvalid()
		{
			var metric = DynatraceMetricsFactory.CreateDoubleGauge("!@#$", 3.4, timestamp: TestDatetime);
			new DynatraceMetricsSerializer(Logger).SerializeMetric(metric).Should()
				.Be("_ gauge,3.4 " + TestTimestamp);
		}

		[Fact]
		public void TestMetricKeyEmpty()
		{
			var serializer = new DynatraceMetricsSerializer(Logger);
			FluentActions.Invoking(() => serializer.SerializeMetric(DynatraceMetricsFactory.CreateLongGauge("", 3))).Should()
				.Throw<DynatraceMetricException>().WithMessage("Metric name can't be null or empty.");
			FluentActions.Invoking(() => serializer.SerializeMetric(DynatraceMetricsFactory.CreateLongGauge(null, 3))).Should()
				.Throw<DynatraceMetricException>().WithMessage("Metric name can't be null or empty.");
		}
		
		// [Fact]
		// public void TestMetricLineTooLong()
		// {
		// 	// TODO: cannot be tested currently since its impossible to create a line that exceeds the line length limit
		// }

		[Fact]
		public void TestMetricTimestampInvalid()
		{
			var serializer = new DynatraceMetricsSerializer(Logger);
			// 01. 01. 1999
			var before2000 = DynatraceMetricsFactory.CreateLongGauge("before-2000", 3, timestamp: new DateTimeOffset(new DateTime(1999, 01, 01)));
			// 01. 01. 3500
			var after3000 = DynatraceMetricsFactory.CreateLongGauge("after-3000", 3, timestamp: new DateTimeOffset(new DateTime(3500, 01, 01)));
			var minDate = DynatraceMetricsFactory.CreateLongGauge("min", 3, timestamp: DateTimeOffset.MinValue);
			var maxDate = DynatraceMetricsFactory.CreateLongGauge("max", 3, timestamp: DateTimeOffset.MinValue);

			serializer.SerializeMetric(before2000).Should().Be("before-2000 gauge,3");
			serializer.SerializeMetric(after3000).Should().Be("after-3000 gauge,3");
			serializer.SerializeMetric(minDate).Should().Be("min gauge,3");
			serializer.SerializeMetric(maxDate).Should().Be("max gauge,3");
		}

		[Fact]
		public void TestDefaultTimestamp()
		{
			var serializer = new DynatraceMetricsSerializer(Logger);
			var implicitDefault = DynatraceMetricsFactory.CreateLongGauge("implicit", 3);
			var explicitDefault = DynatraceMetricsFactory.CreateLongGauge("explicit", 3, timestamp: default);
			var setNull = DynatraceMetricsFactory.CreateLongGauge("set-null", 3, timestamp: null);

			serializer.SerializeMetric(implicitDefault).Should().Be("implicit gauge,3");
			serializer.SerializeMetric(explicitDefault).Should().Be("explicit gauge,3");
			serializer.SerializeMetric(setNull).Should().Be("set-null gauge,3");
		}

		[Fact]
		public void TestAllDefaultValues() => FluentActions.Invoking(() => new DynatraceMetricsSerializer()).Should().NotThrow();
	}
}
