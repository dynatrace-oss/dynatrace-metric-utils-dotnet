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

using System.Collections.Generic;
using System.Linq;
using Xunit;
using FluentAssertions;
using System;

namespace Dynatrace.MetricUtils.Tests
{
	public class MetricsFactoryTests
	{
		private static IEnumerable<KeyValuePair<string, string>> testDims =
			new List<KeyValuePair<string, string>> {
				new KeyValuePair<string, string>("dim1", "val1"),
				new KeyValuePair<string, string>("dim2", "val2")
			};

		private static IEnumerable<KeyValuePair<string, string>> emptyDims = Enumerable.Empty<KeyValuePair<string, string>>();

		private static DateTime testTimestamp = new DateTime(2021, 1, 1, 6, 00, 00);
		private static DateTimeOffset testTimeOffset = new DateTimeOffset(testTimestamp.ToLocalTime());

		[Fact]
		public void TestLongTotalCounter()
		{
			Metric metric = MetricsFactory.CreateLongTotalCounter("mymetric", testDims, 100);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,100");
			// within 50 ms of now and in the past.
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestLongTotalCounterTimestamp()
		{
			Metric metric = MetricsFactory.CreateLongTotalCounter("mymetric", testDims, 100, testTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,100");
			metric.Timestamp.Should().Be(testTimestamp);
		}

		[Fact]
		public void TestLongTotalCounterEmptyDimensions()
		{
			Metric metric = MetricsFactory.CreateLongTotalCounter("mymetric", emptyDims, 100, testTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("count,100");
			metric.Timestamp.Should().Be(testTimestamp);
		}

		[Fact]
		public void TestLongDeltaCounter()
		{
			var metric = MetricsFactory.CreateLongDeltaCounter("mymetric", testDims, 100);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,delta=100");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestLongDeltaCounterTimestamp()
		{

			var metric = MetricsFactory.CreateLongDeltaCounter("mymetric", testDims, 100, testTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,delta=100");
			metric.Timestamp.Should().Be(testTimestamp);
		}

		[Fact]
		public void TestLongDeltaCounterEmptyDimensions()
		{
			var metric = MetricsFactory.CreateLongDeltaCounter("mymetric", emptyDims, 100, testTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("count,delta=100");
			metric.Timestamp.Should().Be(testTimestamp);
		}


		[Fact]
		public void TestLongGauge()
		{
			var metric = MetricsFactory.CreateLongGauge("mymetric", testDims, 100);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("gauge,100");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestLongGaugeTimestamp()
		{
			var metric = MetricsFactory.CreateLongGauge("mymetric", testDims, 100, testTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("gauge,100");
			metric.Timestamp.Should().Be(testTimestamp);
		}

		[Fact]
		public void TestLongGaugeEmptyDimensions()
		{
			var metric = MetricsFactory.CreateLongGauge("mymetric", emptyDims, 100, testTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("gauge,100");
			metric.Timestamp.Should().Be(testTimestamp);
		}

		[Fact]
		public void TestLongSummary()
		{
			var metric = MetricsFactory.CreateLongSummary("mymetric", testDims, 2, 5, 12, 4);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("gauge,min=2,max=5,sum=12,count=4");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestLongSummaryTimestamp()
		{
			var metric = MetricsFactory.CreateLongSummary("mymetric", testDims, 2, 5, 12, 4, testTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("gauge,min=2,max=5,sum=12,count=4");
			metric.Timestamp.Should().Be(testTimestamp);
		}

		[Fact]
		public void TestLongSummaryEmptyDimensions()
		{
			var metric = MetricsFactory.CreateLongSummary("mymetric", emptyDims, 2, 5, 12, 4, testTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("gauge,min=2,max=5,sum=12,count=4");
			metric.Timestamp.Should().Be(testTimestamp);
		}

		[Fact]
		public void TestLongSummaryInvalidNumbers()
		{
			FluentActions.Invoking(() => MetricsFactory.CreateLongSummary("mymetric", testDims, 14, 5, 12, 4))
				.Should().Throw<MetricException>().WithMessage("Min cannot be larger than max.");

			FluentActions.Invoking(() => MetricsFactory.CreateLongSummary("mymetric", testDims, 2, 5, 12, -1))
				.Should().Throw<MetricException>().WithMessage("Count cannot be less than 0.");
		}

		[Fact]
		public void TestDoubleTotalCounter()
		{
			Metric metric = MetricsFactory.CreateDoubleTotalCounter("mymetric", testDims, 100.123);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,100.123");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestDoubleTotalCounterTimestamp()
		{
			Metric metric = MetricsFactory.CreateDoubleTotalCounter("mymetric", testDims, 100.123, testTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,100.123");
			metric.Timestamp.Should().Be(testTimestamp);
		}

		[Fact]
		public void TestDoubleTotalCounterNoDimensions()
		{
			Metric metric = MetricsFactory.CreateDoubleTotalCounter("mymetric", emptyDims, 100.123, testTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("count,100.123");
			metric.Timestamp.Should().Be(testTimestamp);
		}

		[Fact]
		public void TestDoubleTotalCounterInvalidNumbers()
		{
			FluentActions.Invoking(() => MetricsFactory.CreateDoubleTotalCounter("mymetric", testDims, double.NegativeInfinity))
			.Should().Throw<MetricException>().WithMessage("Value is infinite.");

			FluentActions.Invoking(() => MetricsFactory.CreateDoubleTotalCounter("mymetric", testDims, -double.NegativeInfinity))
			.Should().Throw<MetricException>().WithMessage("Value is infinite.");

			FluentActions.Invoking(() => MetricsFactory.CreateDoubleTotalCounter("mymetric", testDims, double.NaN))
			.Should().Throw<MetricException>().WithMessage("Value is NaN.");
		}

		[Fact]
		public void TestDoubleDeltaCounter()
		{
			Metric metric = MetricsFactory.CreateDoubleDeltaCounter("mymetric", testDims, 100.123);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,delta=100.123");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestDoubleDeltaCounterTimestamp()
		{
			Metric metric = MetricsFactory.CreateDoubleDeltaCounter("mymetric", testDims, 100.123, testTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,delta=100.123");
			metric.Timestamp.Should().Be(testTimestamp);
		}

		[Fact]
		public void TestDoubleDeltaCounterNoDimensions()
		{
			Metric metric = MetricsFactory.CreateDoubleDeltaCounter("mymetric", emptyDims, 100.123, testTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("count,delta=100.123");
			metric.Timestamp.Should().Be(testTimestamp);
		}

		[Fact]
		public void TestDoubleDeltaCounterInvalidNumbers()
		{
			FluentActions.Invoking(() => MetricsFactory.CreateDoubleDeltaCounter("mymetric", testDims, double.NegativeInfinity))
			.Should().Throw<MetricException>().WithMessage("Value is infinite.");

			FluentActions.Invoking(() => MetricsFactory.CreateDoubleDeltaCounter("mymetric", testDims, -double.NegativeInfinity))
			.Should().Throw<MetricException>().WithMessage("Value is infinite.");

			FluentActions.Invoking(() => MetricsFactory.CreateDoubleDeltaCounter("mymetric", testDims, double.NaN))
			.Should().Throw<MetricException>().WithMessage("Value is NaN.");
		}

		[Fact]
		public void TestDoubleGauge()
		{
			var metric = MetricsFactory.CreateDoubleGauge("mymetric", testDims, 123.456);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("gauge,123.456");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		// [Fact]
		// public void TestDoubleGauge()
		// {
		// }

		// [Fact]
		// public void TestDoubleSummary()
		// {
		// }

		// [Fact]
		// public void TestDoubleSummary()
		// {
		// }

		// TODO test invalid names
	}
}
