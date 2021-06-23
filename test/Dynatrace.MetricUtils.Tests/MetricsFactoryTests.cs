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

		private static readonly DateTime TestTimestamp = new DateTime(2021, 1, 1, 6, 00, 00);

		[Fact]
		public void TestLongTotalCounter()
		{
			Metric metric = MetricsFactory.CreateLongTotalCounter("mymetric", 100, testDims);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,100");
			// within 50 ms of now and in the past.
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestLongTotalCounterTimestamp()
		{
			Metric metric = MetricsFactory.CreateLongTotalCounter("mymetric", 100, testDims, TestTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,100");
			metric.Timestamp.Should().Be(TestTimestamp);
		}

		[Fact]
		public void TestLongTotalCounterMinimal()
		{
			Metric metric = MetricsFactory.CreateLongTotalCounter("mymetric", 100);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().NotBeNull().And.BeEmpty();
			metric.Value.Serialize().Should().Be("count,100");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestLongDeltaCounter()
		{
			var metric = MetricsFactory.CreateLongDeltaCounter("mymetric", 100, testDims);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,delta=100");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestLongDeltaCounterTimestamp()
		{

			var metric = MetricsFactory.CreateLongDeltaCounter("mymetric", 100, testDims, TestTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,delta=100");
			metric.Timestamp.Should().Be(TestTimestamp);
		}

		[Fact]
		public void TestLongDeltaCounterMinimal()
		{
			var metric = MetricsFactory.CreateLongDeltaCounter("mymetric", 100);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("count,delta=100");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}


		[Fact]
		public void TestLongGauge()
		{
			var metric = MetricsFactory.CreateLongGauge("mymetric", 100, testDims);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("gauge,100");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestLongGaugeTimestamp()
		{
			var metric = MetricsFactory.CreateLongGauge("mymetric", 100, testDims, TestTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("gauge,100");
			metric.Timestamp.Should().Be(TestTimestamp);
		}

		[Fact]
		public void TestLongGaugeMinimal()
		{
			var metric = MetricsFactory.CreateLongGauge("mymetric", 100);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("gauge,100");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestLongSummary()
		{
			var metric = MetricsFactory.CreateLongSummary("mymetric", 2, 5, 12, 4, testDims);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("gauge,min=2,max=5,sum=12,count=4");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestLongSummaryTimestamp()
		{
			var metric = MetricsFactory.CreateLongSummary("mymetric", 2, 5, 12, 4, testDims, TestTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("gauge,min=2,max=5,sum=12,count=4");
			metric.Timestamp.Should().Be(TestTimestamp);
		}

		[Fact]
		public void TestLongSummaryMinimal()
		{
			var metric = MetricsFactory.CreateLongSummary("mymetric", 2, 5, 12, 4);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("gauge,min=2,max=5,sum=12,count=4");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestLongSummaryInvalidNumbers()
		{
			FluentActions.Invoking(() => MetricsFactory.CreateLongSummary("mymetric", 14, 5, 12, 4))
				.Should().Throw<MetricException>().WithMessage("Min cannot be larger than max.");

			FluentActions.Invoking(() => MetricsFactory.CreateLongSummary("mymetric", 2, 5, 12, -1))
				.Should().Throw<MetricException>().WithMessage("Count cannot be less than 0.");
		}

		[Fact]
		public void TestDoubleTotalCounter()
		{
			Metric metric = MetricsFactory.CreateDoubleTotalCounter("mymetric", 100.123, testDims);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,100.123");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestDoubleTotalCounterTimestamp()
		{
			Metric metric = MetricsFactory.CreateDoubleTotalCounter("mymetric", 100.123, testDims, TestTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,100.123");
			metric.Timestamp.Should().Be(TestTimestamp);
		}

		[Fact]
		public void TestDoubleTotalCounterMinimal()
		{
			Metric metric = MetricsFactory.CreateDoubleTotalCounter("mymetric", 100.123);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("count,100.123");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestDoubleTotalCounterInvalidNumbers()
		{
			FluentActions.Invoking(() => MetricsFactory.CreateDoubleTotalCounter("mymetric", double.NegativeInfinity))
			.Should().Throw<MetricException>().WithMessage("Value is infinite.");

			FluentActions.Invoking(() => MetricsFactory.CreateDoubleTotalCounter("mymetric", -double.NegativeInfinity))
			.Should().Throw<MetricException>().WithMessage("Value is infinite.");

			FluentActions.Invoking(() => MetricsFactory.CreateDoubleTotalCounter("mymetric", double.NaN))
			.Should().Throw<MetricException>().WithMessage("Value is NaN.");
		}

		[Fact]
		public void TestDoubleDeltaCounter()
		{
			Metric metric = MetricsFactory.CreateDoubleDeltaCounter("mymetric", 100.123, testDims);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,delta=100.123");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestDoubleDeltaCounterTimestamp()
		{
			Metric metric = MetricsFactory.CreateDoubleDeltaCounter("mymetric", 100.123, testDims, TestTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("count,delta=100.123");
			metric.Timestamp.Should().Be(TestTimestamp);
		}

		[Fact]
		public void TestDoubleDeltaCounterMinimal()
		{
			Metric metric = MetricsFactory.CreateDoubleDeltaCounter("mymetric", 100.123);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("count,delta=100.123");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestDoubleDeltaCounterInvalidNumbers()
		{
			FluentActions.Invoking(() => MetricsFactory.CreateDoubleDeltaCounter("mymetric", double.NegativeInfinity))
			.Should().Throw<MetricException>().WithMessage("Value is infinite.");

			FluentActions.Invoking(() => MetricsFactory.CreateDoubleDeltaCounter("mymetric", -double.NegativeInfinity))
			.Should().Throw<MetricException>().WithMessage("Value is infinite.");

			FluentActions.Invoking(() => MetricsFactory.CreateDoubleDeltaCounter("mymetric", double.NaN))
			.Should().Throw<MetricException>().WithMessage("Value is NaN.");
		}

		[Fact]
		public void TestDoubleGauge()
		{
			var metric = MetricsFactory.CreateDoubleGauge("mymetric", 123.456, testDims);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("gauge,123.456");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}
		[Fact]
		public void TestDoubleGaugeTimestamp()
		{
			var metric = MetricsFactory.CreateDoubleGauge("mymetric", 123.456, testDims, TestTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("gauge,123.456");
			metric.Timestamp.Should().Be(TestTimestamp);
		}

		[Fact]
		public void TestDoubleGaugeMinimal()
		{
			var metric = MetricsFactory.CreateDoubleGauge("mymetric", 123.456);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("gauge,123.456");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestDoubleGaugeInvalidNumbers()
		{
			FluentActions.Invoking(() => MetricsFactory.CreateDoubleGauge("mymetric", double.NegativeInfinity))
			.Should().Throw<MetricException>().WithMessage("Value is infinite.");

			FluentActions.Invoking(() => MetricsFactory.CreateDoubleGauge("mymetric", -double.NegativeInfinity))
			.Should().Throw<MetricException>().WithMessage("Value is infinite.");

			FluentActions.Invoking(() => MetricsFactory.CreateDoubleGauge("mymetric", double.NaN))
			.Should().Throw<MetricException>().WithMessage("Value is NaN.");
		}

		[Fact]
		public void TestDoubleSummary()
		{
			var metric = MetricsFactory.CreateDoubleSummary("mymetric", 2.5, 5.7, 12.3, 4, testDims);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("gauge,min=2.5,max=5.7,sum=12.3,count=4");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestDoubleSummaryTimestamp()
		{
			var metric = MetricsFactory.CreateDoubleSummary("mymetric", 2.5, 5.7, 12.3, 4, testDims, TestTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(testDims);
			metric.Value.Serialize().Should().Be("gauge,min=2.5,max=5.7,sum=12.3,count=4");
			metric.Timestamp.Should().Be(TestTimestamp);
		}

		[Fact]
		public void TestDoubleSummaryMinimal()
		{
			var metric = MetricsFactory.CreateDoubleSummary("mymetric", 2.5, 5.7, 12.3, 4);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("gauge,min=2.5,max=5.7,sum=12.3,count=4");
			metric.Timestamp.Should().BeCloseTo(DateTime.Now, 50).And.BeBefore(DateTime.Now);
		}

		[Fact]
		public void TestDoubleSummaryInvalidNumbers()
		{
			FluentActions.Invoking(() => MetricsFactory.CreateDoubleSummary("mymetric", 14.6, 5.3, 12.7, 4))
				.Should().Throw<MetricException>().WithMessage("Min cannot be larger than max.");

			FluentActions.Invoking(() => MetricsFactory.CreateDoubleSummary("mymetric", 2.5, 5.7, 12.3, -1))
				.Should().Throw<MetricException>().WithMessage("Count cannot be less than 0.");
		}

		[Fact]
		public void TestDoubleSummaryNanAndInf()
		{
			var values = new List<double> { 1.1, double.PositiveInfinity, double.NegativeInfinity, double.NaN };
			foreach (var i in values)
			{
				foreach (var j in values)
				{
					foreach (var k in values)
					{
						if (i == 1.1 && j == 1.1 && k == 1.1)
						{
							// if all values are 1.1, this is a valid configuration. Therefore, check that its correctly serialized.
							Metric metric = MetricsFactory.CreateDoubleSummary("mymetric", i, j, k, 1);
							metric.Value.Serialize().Should().Be("gauge,min=1.1,max=1.1,sum=1.1,count=1");
							continue;
						}
						// any of the other configurations are invalid and should throw.
						FluentActions.Invoking(() => MetricsFactory.CreateDoubleSummary("mymetric", i, j, k, 1))
						.Should().Throw<MetricException>();
					}
				}
			}
		}
	}
}
