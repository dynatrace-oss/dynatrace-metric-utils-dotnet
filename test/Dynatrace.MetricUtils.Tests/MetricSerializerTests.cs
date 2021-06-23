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
			new List<KeyValuePair<string, string>> {new("dim1", "value1"), new("dim2", "value2")};

		[Fact]
		public void SerializeLongDeltaCounter()
		{
			var serializer = new MetricSerializer(Logger);

			var serializedWithAllParams =
				serializer.SerializeMetric(
					MetricsFactory.CreateLongDeltaCounter("metric1", 100, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 count,delta=100 " + TestTimestamp + Environment.NewLine);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(MetricsFactory.CreateLongDeltaCounter("metric2", 200, TestDimensions));
			var currTimestampPrefix = "metric2,dim1=value1,dim2=value2 count,delta=200 ";
			serializedWithCurrentTimestamp.Should()
				.StartWith(currTimestampPrefix)
				.And.HaveLength(currTimestampPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);

			var serializedWithMinimalParams =
				serializer.SerializeMetric(MetricsFactory.CreateLongDeltaCounter("metric3", 300));
			var minimalParamsPrefix = "metric3 count,delta=300 ";
			serializedWithMinimalParams.Should()
				.StartWith(minimalParamsPrefix)
				.And.HaveLength(minimalParamsPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);
		}

		[Fact]
		public void SerializeLongTotalCounter()
		{
			var serializer = new MetricSerializer(Logger);

			var serializedWithAllParams =
				serializer.SerializeMetric(
					MetricsFactory.CreateLongTotalCounter("metric1", 100, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 count,100 " + TestTimestamp + Environment.NewLine);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(MetricsFactory.CreateLongTotalCounter("metric2", 200, TestDimensions));
			var currTimestampPrefix = "metric2,dim1=value1,dim2=value2 count,200 ";
			serializedWithCurrentTimestamp.Should()
				.StartWith(currTimestampPrefix)
				.And.HaveLength(currTimestampPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);

			var serializedWithMinimalParams =
				serializer.SerializeMetric(MetricsFactory.CreateLongTotalCounter("metric3", 300));
			var minimalParamsPrefix = "metric3 count,300 ";
			serializedWithMinimalParams.Should()
				.StartWith(minimalParamsPrefix)
				.And.HaveLength(minimalParamsPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);
		}

		[Fact]
		public void SerializeLongGauge()
		{
			var serializer = new MetricSerializer(Logger);

			var serializedWithAllParams =
				serializer.SerializeMetric(
					MetricsFactory.CreateLongGauge("metric1", 100, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 gauge,100 " + TestTimestamp + Environment.NewLine);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(MetricsFactory.CreateLongGauge("metric2", 200, TestDimensions));
			var currTimestampPrefix = "metric2,dim1=value1,dim2=value2 gauge,200 ";
			serializedWithCurrentTimestamp.Should()
				.StartWith(currTimestampPrefix)
				.And.HaveLength(currTimestampPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);

			var serializedWithMinimalParams =
				serializer.SerializeMetric(MetricsFactory.CreateLongGauge("metric3", 300));
			var minimalParamsPrefix = "metric3 gauge,300 ";
			serializedWithMinimalParams.Should()
				.StartWith(minimalParamsPrefix)
				.And.HaveLength(minimalParamsPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);
		}

		[Fact]
		public void SerializeLongSummary()
		{
			var serializer = new MetricSerializer(Logger);
			var serializedWithAllParams =
				serializer.SerializeMetric(
					MetricsFactory.CreateLongSummary("metric1", 1, 3, 7, 4, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 gauge,min=1,max=3,sum=7,count=4 " + TestTimestamp +
				    Environment.NewLine);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(MetricsFactory.CreateLongSummary("metric2", 1, 3, 7, 4, TestDimensions));
			var currTimestampPrefix = "metric2,dim1=value1,dim2=value2 gauge,min=1,max=3,sum=7,count=4 ";
			serializedWithCurrentTimestamp.Should()
				.StartWith(currTimestampPrefix)
				.And.HaveLength(currTimestampPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);

			var serializedWithMinimalParams =
				serializer.SerializeMetric(MetricsFactory.CreateLongSummary("metric3", 1, 3, 7, 4));
			var minimalParamsPrefix = "metric3 gauge,min=1,max=3,sum=7,count=4 ";
			serializedWithMinimalParams.Should()
				.StartWith(minimalParamsPrefix)
				.And.HaveLength(minimalParamsPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);
		}

		[Fact]
		public void SerializeDoubleDeltaCounter()
		{
			var serializer = new MetricSerializer(Logger);

			var serializedWithAllParams =
				serializer.SerializeMetric(
					MetricsFactory.CreateDoubleDeltaCounter("metric1", 123.456, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 count,delta=123.456 " + TestTimestamp + Environment.NewLine);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(MetricsFactory.CreateDoubleDeltaCounter("metric2", 223.456, TestDimensions));
			var currTimestampPrefix = "metric2,dim1=value1,dim2=value2 count,delta=223.456 ";
			serializedWithCurrentTimestamp.Should()
				.StartWith(currTimestampPrefix)
				.And.HaveLength(currTimestampPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);

			var serializedWithMinimalParams =
				serializer.SerializeMetric(MetricsFactory.CreateDoubleDeltaCounter("metric3", 323.456));
			var minimalParamsPrefix = "metric3 count,delta=323.456 ";
			serializedWithMinimalParams.Should()
				.StartWith(minimalParamsPrefix)
				.And.HaveLength(minimalParamsPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);
		}

		[Fact]
		public void SerializeDoubleTotalCounter()
		{
			var serializer = new MetricSerializer(Logger);

			var serializedWithAllParams =
				serializer.SerializeMetric(
					MetricsFactory.CreateDoubleTotalCounter("metric1", 123.456, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 count,123.456 " + TestTimestamp + Environment.NewLine);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(MetricsFactory.CreateDoubleTotalCounter("metric2", 223.456, TestDimensions));
			var currTimestampPrefix = "metric2,dim1=value1,dim2=value2 count,223.456 ";
			serializedWithCurrentTimestamp.Should()
				.StartWith(currTimestampPrefix)
				.And.HaveLength(currTimestampPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);

			var serializedWithMinimalParams =
				serializer.SerializeMetric(MetricsFactory.CreateDoubleTotalCounter("metric3", 323.456));
			var minimalParamsPrefix = "metric3 count,323.456 ";
			serializedWithMinimalParams.Should()
				.StartWith(minimalParamsPrefix)
				.And.HaveLength(minimalParamsPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);
		}

		[Fact]
		public void SerializeDoubleGauge()
		{
			var serializer = new MetricSerializer(Logger);

			var serializedWithAllParams =
				serializer.SerializeMetric(
					MetricsFactory.CreateDoubleGauge("metric1", 123.456, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 gauge,123.456 " + TestTimestamp + Environment.NewLine);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(MetricsFactory.CreateDoubleGauge("metric2", 223.456, TestDimensions));
			var currTimestampPrefix = "metric2,dim1=value1,dim2=value2 gauge,223.456 ";
			serializedWithCurrentTimestamp.Should()
				.StartWith(currTimestampPrefix)
				.And.HaveLength(currTimestampPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);

			var serializedWithMinimalParams =
				serializer.SerializeMetric(MetricsFactory.CreateDoubleGauge("metric3", 323.456));
			var minimalParamsPrefix = "metric3 gauge,323.456 ";
			serializedWithMinimalParams.Should()
				.StartWith(minimalParamsPrefix)
				.And.HaveLength(minimalParamsPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);
		}

		[Fact]
		public void SerializeDoubleSummary()
		{
			var serializer = new MetricSerializer(Logger);
			var serializedWithAllParams =
				serializer.SerializeMetric(
					MetricsFactory.CreateDoubleSummary("metric1", 1.2, 3.4, 7.8, 4, TestDimensions, TestDatetime));
			serializedWithAllParams.Should()
				.Be("metric1,dim1=value1,dim2=value2 gauge,min=1.2,max=3.4,sum=7.8,count=4 " + TestTimestamp +
				    Environment.NewLine);

			var serializedWithCurrentTimestamp =
				serializer.SerializeMetric(
					MetricsFactory.CreateDoubleSummary("metric2", 1.2, 3.4, 7.8, 4, TestDimensions));
			var currTimestampPrefix = "metric2,dim1=value1,dim2=value2 gauge,min=1.2,max=3.4,sum=7.8,count=4 ";
			serializedWithCurrentTimestamp.Should()
				.StartWith(currTimestampPrefix)
				.And.HaveLength(currTimestampPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);

			var serializedWithMinimalParams =
				serializer.SerializeMetric(MetricsFactory.CreateDoubleSummary("metric3", 1.2, 3.4, 7.8, 4));
			var minimalParamsPrefix = "metric3 gauge,min=1.2,max=3.4,sum=7.8,count=4 ";
			serializedWithMinimalParams.Should()
				.StartWith(minimalParamsPrefix)
				.And.HaveLength(minimalParamsPrefix.Length + TestTimestamp.Length + Environment.NewLine.Length);
		}

		[Fact]
		public void TestDimensionValuesNormalized()
		{
			var dims = new List<KeyValuePair<string, string>> {new("dim1", "\\=\" ==")};
			var metric = MetricsFactory.CreateLongDeltaCounter("metric1", 100, dims, TestDatetime);

			var serialized = new MetricSerializer(Logger).SerializeMetric(metric);
			serialized.Should()
				.Be("metric1,dim1=\\\\\\=\"\\ \\=\\= count,delta=100 " + TestTimestamp + Environment.NewLine);
		}

		[Fact]
		public void TestPrefix()
		{
			var metric = MetricsFactory.CreateLongDeltaCounter("metric", 100, timestamp: TestDatetime);

			var serialized = new MetricSerializer(Logger, "prefix").SerializeMetric(metric);
			serialized.Should()
				.Be("prefix.metric count,delta=100 " + TestTimestamp + Environment.NewLine);
		}

		[Fact]
		public void TestPrefixWithTrailingDot()
		{
			var metric = MetricsFactory.CreateLongDeltaCounter("metric", 100, timestamp: TestDatetime);
			var serializedWithTrailingPrefixDot = new MetricSerializer(Logger, "prefix.").SerializeMetric(metric);
			serializedWithTrailingPrefixDot.Should()
				.Be("prefix.metric count,delta=100 " + TestTimestamp + Environment.NewLine);
		}

		[Fact]
		public void TestWithDefaultDimensions()
		{
			var metric = MetricsFactory.CreateLongDeltaCounter("metric", 100, TestDimensions, TestDatetime);
			var defaultDimensions = new List<KeyValuePair<string, string>>
			{
				new("default1", "value1"), new("default2", "value2")
			};

			var serialized = new MetricSerializer(Logger, defaultDimensions: defaultDimensions).SerializeMetric(metric);
			serialized.Should()
				.Be("metric,default1=value1,default2=value2,dim1=value1,dim2=value2 count,delta=100 " + TestTimestamp +
				    Environment.NewLine);
		}

		[Fact]
		public void TestMetricsSource()
		{
			var metric = MetricsFactory.CreateLongDeltaCounter("metric", 100, null, TestDatetime);

			// the use case probably used most often
			new MetricSerializer(Logger, metricsSource: "opentelemetry").SerializeMetric(metric)
				.Should().Be("metric,dt.metrics.source=opentelemetry count,delta=100 " + TestTimestamp +
				             Environment.NewLine);

			// empty source will not be added
			new MetricSerializer(Logger, metricsSource: "").SerializeMetric(metric)
				.Should().Be("metric count,delta=100 " + TestTimestamp + Environment.NewLine);

			// invalid characters in source will be escaped:
			new MetricSerializer(Logger, metricsSource: "esc\\ape=this\"").SerializeMetric(metric)
				.Should().Be("metric,dt.metrics.source=esc\\\\ape\\=this\" count,delta=100 " + TestTimestamp +
				             Environment.NewLine);
		}

		[Fact]
		public void TestDimensionPrecedence()
		{
			var defaultDimensions = new List<KeyValuePair<string, string>>
			{
				new("dim1", "default1"), new("dim2", "default2"), new("dim3", "default3")
			};

			var metricDimensions =
				new List<KeyValuePair<string, string>> {new("dim2", "metric2"), new("dim3", "metric3")};

			var staticDimensions = new List<KeyValuePair<string, string>> {new("dim3", "static3")};

			var metric = MetricsFactory.CreateLongDeltaCounter("metric", 100, metricDimensions, TestDatetime);

			// using the internal constructor that accepts the static dimensions for testing:
			var serializer = new MetricSerializer(Logger, "prefix", defaultDimensions, staticDimensions);

			serializer.SerializeMetric(metric)
				.Should().Be("prefix.metric,dim1=default1,dim2=metric2,dim3=static3 count,delta=100 " + TestTimestamp +
				             Environment.NewLine);
		}

		[Fact]
		public void TestMergeDimensionsLeavesOneListAsIs()
		{
			var defaultDimensions = new List<KeyValuePair<string, string>>
			{
				new("dim1", "default1"), new("dim2", "default2"), new("dim3", "default3")
			};

			MetricSerializer.MergeDimensions(defaultDimensions).Should().Equal(defaultDimensions);
		}


		[Fact]
		public void TestMergeDimensionsOverwritesInCorrectOrder()
		{
			var defaultDimensions = new List<KeyValuePair<string, string>>
			{
				new("dim1", "default1"), new("dim2", "default2"), new("dim3", "default3")
			};

			var metricDimensions =
				new List<KeyValuePair<string, string>> {new("dim2", "metric2"), new("dim3", "metric3")};

			var staticDimensions = new List<KeyValuePair<string, string>> {new("dim3", "static3")};

			MetricSerializer.MergeDimensions(defaultDimensions, metricDimensions, staticDimensions).Should()
				.Equal(new List<KeyValuePair<string, string>>
				{
					new("dim1", "default1"), new("dim2", "metric2"), new("dim3", "static3")
				});
		}

		[Fact]
		public void TestMergeDimensionDisjunctKeys()
		{
			var someDimensions =
				new List<KeyValuePair<string, string>>
				{
					new("some1", "val1"), new("some2", "val2"), new("some3", "val3")
				};
			var otherDimensions = new List<KeyValuePair<string, string>>
			{
				new("other1", "val1"), new("other2", "val2"), new("other3", "val3")
			};

			MetricSerializer.MergeDimensions(someDimensions, otherDimensions)
				.Should().Equal(
					new List<KeyValuePair<string, string>>
					{
						new("some1", "val1"),
						new("some2", "val2"),
						new("some3", "val3"),
						new("other1", "val1"),
						new("other2", "val2"),
						new("other3", "val3")
					}
				);
		}
	}
}
