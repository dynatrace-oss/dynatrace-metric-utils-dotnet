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
	public class MetricSerializerTests
	{
		private static readonly ILogger<MetricSerializerTests> Logger = NullLogger<MetricSerializerTests>.Instance;

		// use the same timestamp for all tests
		private static readonly DateTime TestDatetime = DateTime.Now;

		// string representation of the above DateTime in Unix milliseconds
		private static readonly string TestTimestamp =
			$"{new DateTimeOffset(TestDatetime.ToLocalTime()).ToUnixTimeMilliseconds()}";

		private static readonly IEnumerable<KeyValuePair<string, string>> TestDimensions =
			new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("dim1", "value1"),
				new KeyValuePair<string, string>("dim2", "value2")
			};

		[Fact]
		public void SerializeLongCounter()
		{
			var serializer = new MetricsSerializer(Logger);

			var serializedWithAllParams =
				serializer.SerializeMetric(
					MetricsFactory.CreateLongCounter("metric1", 100, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 count,delta=100 " + TestTimestamp);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(MetricsFactory.CreateLongCounter("metric2", 200, TestDimensions));
			serializedWithCurrentTimestamp.Should().Be("metric2,dim1=value1,dim2=value2 count,delta=200");

			var serializedWithMinimalParams =
				serializer.SerializeMetric(MetricsFactory.CreateLongCounter("metric3", 300));
			serializedWithMinimalParams.Should().Be("metric3 count,delta=300");
		}

		[Fact]
		public void SerializeLongGauge()
		{
			var serializer = new MetricsSerializer(Logger);

			var serializedWithAllParams =
				serializer.SerializeMetric(
					MetricsFactory.CreateLongGauge("metric1", 100, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 gauge,100 " + TestTimestamp);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(MetricsFactory.CreateLongGauge("metric2", 200, TestDimensions));
			serializedWithCurrentTimestamp.Should().Be("metric2,dim1=value1,dim2=value2 gauge,200");

			var serializedWithMinimalParams =
				serializer.SerializeMetric(MetricsFactory.CreateLongGauge("metric3", 300));
			serializedWithMinimalParams.Should().Be("metric3 gauge,300");
		}

		[Fact]
		public void SerializeLongSummary()
		{
			var serializer = new MetricsSerializer(Logger);
			var serializedWithAllParams =
				serializer.SerializeMetric(
					MetricsFactory.CreateLongSummary("metric1", 1, 3, 7, 4, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 gauge,min=1,max=3,sum=7,count=4 " + TestTimestamp);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(MetricsFactory.CreateLongSummary("metric2", 1, 3, 7, 4, TestDimensions));
			serializedWithCurrentTimestamp.Should()
				.Be("metric2,dim1=value1,dim2=value2 gauge,min=1,max=3,sum=7,count=4");

			var serializedWithMinimalParams =
				serializer.SerializeMetric(MetricsFactory.CreateLongSummary("metric3", 1, 3, 7, 4));
			serializedWithMinimalParams.Should().Be("metric3 gauge,min=1,max=3,sum=7,count=4");
		}

		[Fact]
		public void SerializeDoubleCounter()
		{
			var serializer = new MetricsSerializer(Logger);

			var serializedWithAllParams =
				serializer.SerializeMetric(
					MetricsFactory.CreateDoubleCounter("metric1", 123.456, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 count,delta=123.456 " + TestTimestamp);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(MetricsFactory.CreateDoubleCounter("metric2", 223.456, TestDimensions));
			serializedWithCurrentTimestamp.Should().Be("metric2,dim1=value1,dim2=value2 count,delta=223.456");

			var serializedWithMinimalParams =
				serializer.SerializeMetric(MetricsFactory.CreateDoubleCounter("metric3", 323.456));
			serializedWithMinimalParams.Should().Be("metric3 count,delta=323.456");
		}

		[Fact]
		public void SerializeDoubleGauge()
		{
			var serializer = new MetricsSerializer(Logger);

			var serializedWithAllParams =
				serializer.SerializeMetric(
					MetricsFactory.CreateDoubleGauge("metric1", 123.456, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 gauge,123.456 " + TestTimestamp);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(MetricsFactory.CreateDoubleGauge("metric2", 223.456, TestDimensions));
			serializedWithCurrentTimestamp.Should().Be("metric2,dim1=value1,dim2=value2 gauge,223.456");

			var serializedWithMinimalParams =
				serializer.SerializeMetric(MetricsFactory.CreateDoubleGauge("metric3", 323.456));
			serializedWithMinimalParams.Should().Be("metric3 gauge,323.456");
		}

		[Fact]
		public void SerializeDoubleSummary()
		{
			var serializer = new MetricsSerializer(Logger);
			var serializedWithAllParams =
				serializer.SerializeMetric(
					MetricsFactory.CreateDoubleSummary("metric1", 1.2, 3.4, 7.8, 4, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 gauge,min=1.2,max=3.4,sum=7.8,count=4 " + TestTimestamp);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(
					MetricsFactory.CreateDoubleSummary("metric2", 1.2, 3.4, 7.8, 4, TestDimensions));
			serializedWithCurrentTimestamp.Should()
				.Be("metric2,dim1=value1,dim2=value2 gauge,min=1.2,max=3.4,sum=7.8,count=4");

			var serializedWithMinimalParams =
				serializer.SerializeMetric(MetricsFactory.CreateDoubleSummary("metric3", 1.2, 3.4, 7.8, 4));
			serializedWithMinimalParams.Should().Be("metric3 gauge,min=1.2,max=3.4,sum=7.8,count=4");
		}

		[Fact]
		public void TestDimensionValuesNormalized()
		{
			var dims = new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("dim1", "\\=\" ==")};
			var metric = MetricsFactory.CreateLongCounter("metric1", 100, dims, TestDatetime);

			var serialized = new MetricsSerializer(Logger).SerializeMetric(metric);
			serialized.Should()
				.Be("metric1,dim1=\\\\\\=\\\"\\ \\=\\= count,delta=100 " + TestTimestamp);
		}

		[Fact]
		public void TestPrefix()
		{
			var metric = MetricsFactory.CreateLongCounter("metric", 100, timestamp: TestDatetime);

			var serialized = new MetricsSerializer(Logger, "prefix").SerializeMetric(metric);
			serialized.Should()
				.Be("prefix.metric count,delta=100 " + TestTimestamp);
		}

		[Fact]
		public void TestPrefixWithTrailingDot()
		{
			var metric = MetricsFactory.CreateLongCounter("metric", 100, timestamp: TestDatetime);
			var serializedWithTrailingPrefixDot = new MetricsSerializer(Logger, "prefix.").SerializeMetric(metric);
			serializedWithTrailingPrefixDot.Should()
				.Be("prefix.metric count,delta=100 " + TestTimestamp);
		}

		[Fact]
		public void TestWithDefaultDimensions()
		{
			var metric = MetricsFactory.CreateLongCounter("metric", 100, TestDimensions, TestDatetime);
			var defaultDimensions = new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("default1", "value1"),
				new KeyValuePair<string, string>("default2", "value2")
			};

			var serialized =
				new MetricsSerializer(Logger, defaultDimensions: defaultDimensions).SerializeMetric(metric);
			serialized.Should()
				.Be("metric,default1=value1,default2=value2,dim1=value1,dim2=value2 count,delta=100 " + TestTimestamp);
		}

		[Fact]
		public void TestMetricsSource()
		{
			var metric = MetricsFactory.CreateLongCounter("metric", 100, null, TestDatetime);

			// the use case probably used most often
			new MetricsSerializer(Logger, metricsSource: "opentelemetry").SerializeMetric(metric)
				.Should().Be("metric,dt.metrics.source=opentelemetry count,delta=100 " + TestTimestamp);

			// empty source will not be added
			new MetricsSerializer(Logger, metricsSource: "").SerializeMetric(metric)
				.Should().Be("metric count,delta=100 " + TestTimestamp);

			// invalid characters in source will be escaped:
			new MetricsSerializer(Logger, metricsSource: "esc\\ape=this\"").SerializeMetric(metric)
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
				new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("dim3", "static3")};

			var metric = MetricsFactory.CreateLongCounter("metric", 100, metricDimensions, TestDatetime);

			// using the internal constructor that accepts the static dimensions for testing:
			var serializer = new MetricsSerializer(Logger, "prefix", defaultDimensions, staticDimensions);

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

			MetricsSerializer.MergeDimensions(defaultDimensions).Should().Equal(defaultDimensions);
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
				new List<KeyValuePair<string, string>> {new KeyValuePair<string, string>("dim3", "static3")};

			MetricsSerializer.MergeDimensions(defaultDimensions, metricDimensions, staticDimensions).Should()
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

			MetricsSerializer.MergeDimensions(someDimensions, otherDimensions)
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
			var metric = MetricsFactory.CreateDoubleGauge("!@#$", 3.4, timestamp: TestDatetime);
			new MetricsSerializer(Logger).SerializeMetric(metric).Should()
				.Be("_ gauge,3.4 " + TestTimestamp);
		}

		[Fact]
		public void TestMetricKeyEmpty()
		{
			var serializer = new MetricsSerializer(Logger);
			FluentActions.Invoking(() => serializer.SerializeMetric(MetricsFactory.CreateLongGauge("", 3))).Should()
				.Throw<MetricException>().WithMessage("Metric name can't be null or empty.");
			FluentActions.Invoking(() => serializer.SerializeMetric(MetricsFactory.CreateLongGauge(null, 3))).Should()
				.Throw<MetricException>().WithMessage("Metric name can't be null or empty.");
		}

		[Fact]
		public void TestMetricLineTooLong()
		{
			var dimensions = new List<KeyValuePair<string, string>>();
			// 20 dimensions of ~ 100 characters should result in lines with more than 2000 characters
			for (var i = 0; i < 20; i++)
			{
				// creates a dimension that takes up a little more than 100 characters
				dimensions.Add(new KeyValuePair<string, string>(new string('a', 50) + i, new string('b', 50) + i));
			}

			var serializer = new MetricsSerializer(Logger);
			var metric = MetricsFactory.CreateLongGauge("metric", 4, dimensions);

			FluentActions.Invoking(() => serializer.SerializeMetric(metric)).Should().Throw<MetricException>()
				.WithMessage("Metric line exceeds line length of 2000 characters (Metric name: 'metric').");
		}

		[Fact]
		public void TestMetricTimestampInvalid()
		{
			var serializer = new MetricsSerializer(Logger);
			// 01. 01. 1999
			var before2000 = MetricsFactory.CreateLongGauge("before-2000", 3, timestamp: new DateTime(1999, 01, 01));
			// 01. 01. 3500
			var after3000 = MetricsFactory.CreateLongGauge("after-3000", 3, timestamp: new DateTime(3500, 01, 01));
			var minDate = MetricsFactory.CreateLongGauge("min", 3, timestamp: DateTime.MinValue);
			var maxDate = MetricsFactory.CreateLongGauge("max", 3, timestamp: DateTime.MinValue);

			serializer.SerializeMetric(before2000).Should().Be("before-2000 gauge,3");
			serializer.SerializeMetric(after3000).Should().Be("after-3000 gauge,3");
			serializer.SerializeMetric(minDate).Should().Be("min gauge,3");
			serializer.SerializeMetric(maxDate).Should().Be("max gauge,3");
		}

		[Fact]
		public void TestDefaultTimestamp()
		{
			var serializer = new MetricsSerializer(Logger);
			var implicitDefault = MetricsFactory.CreateLongGauge("implicit", 3);
			var explicitDefault = MetricsFactory.CreateLongGauge("explicit", 3, timestamp: default);
			var setNull = MetricsFactory.CreateLongGauge("set-null", 3, timestamp: null);

			serializer.SerializeMetric(implicitDefault).Should().Be("implicit gauge,3");
			serializer.SerializeMetric(explicitDefault).Should().Be("explicit gauge,3");
			serializer.SerializeMetric(setNull).Should().Be("set-null gauge,3");
		}

		[Fact]
		public void TestAllDefaultValues()
		{
			FluentActions.Invoking(() => new MetricsSerializer()).Should().NotThrow();
		}
	}
}
