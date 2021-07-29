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
using Xunit;

namespace Dynatrace.MetricUtils.Tests
{
	public class MetricsFactoryTests
	{
		private static readonly IEnumerable<KeyValuePair<string, string>> TestDims =
			new List<KeyValuePair<string, string>>
			{
				new KeyValuePair<string, string>("dim1", "val1"), new KeyValuePair<string, string>("dim2", "val2")
			};

		private static readonly DateTime TestTimestamp = new DateTime(2021, 1, 1, 6, 00, 00);

		[Fact]
		public void TestLongCounter()
		{
			var metric = MetricsFactory.CreateLongCounterDelta("mymetric", 100, TestDims);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(TestDims);
			metric.Value.Serialize().Should().Be("count,delta=100");
			metric.Timestamp.Should().BeNull();
		}

		[Fact]
		public void TestLongCounterTimestamp()
		{
			var metric = MetricsFactory.CreateLongCounterDelta("mymetric", 100, TestDims, TestTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(TestDims);
			metric.Value.Serialize().Should().Be("count,delta=100");
			metric.Timestamp.Should().Be(TestTimestamp);
		}

		[Fact]
		public void TestLongCounterMinimal()
		{
			var metric = MetricsFactory.CreateLongCounterDelta("mymetric", 100);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("count,delta=100");
			metric.Timestamp.Should().BeNull();
		}


		[Fact]
		public void TestLongGauge()
		{
			var metric = MetricsFactory.CreateLongGauge("mymetric", 100, TestDims);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(TestDims);
			metric.Value.Serialize().Should().Be("gauge,100");
			metric.Timestamp.Should().BeNull();
		}

		[Fact]
		public void TestLongGaugeTimestamp()
		{
			var metric = MetricsFactory.CreateLongGauge("mymetric", 100, TestDims, TestTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(TestDims);
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
			metric.Timestamp.Should().BeNull();
		}

		[Fact]
		public void TestLongSummary()
		{
			var metric = MetricsFactory.CreateLongSummary("mymetric", 2, 5, 12, 4, TestDims);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(TestDims);
			metric.Value.Serialize().Should().Be("gauge,min=2,max=5,sum=12,count=4");
			metric.Timestamp.Should().BeNull();
		}

		[Fact]
		public void TestLongSummaryTimestamp()
		{
			var metric = MetricsFactory.CreateLongSummary("mymetric", 2, 5, 12, 4, TestDims, TestTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(TestDims);
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
			metric.Timestamp.Should().BeNull();
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
		public void TestDoubleCounter()
		{
			var metric = MetricsFactory.CreateDoubleCounterDelta("mymetric", 100.123, TestDims);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(TestDims);
			metric.Value.Serialize().Should().Be("count,delta=100.123");
			metric.Timestamp.Should().BeNull();
		}

		[Fact]
		public void TestDoubleCounterTimestamp()
		{
			var metric = MetricsFactory.CreateDoubleCounterDelta("mymetric", 100.123, TestDims, TestTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(TestDims);
			metric.Value.Serialize().Should().Be("count,delta=100.123");
			metric.Timestamp.Should().Be(TestTimestamp);
		}

		[Fact]
		public void TestDoubleCounterMinimal()
		{
			var metric = MetricsFactory.CreateDoubleCounterDelta("mymetric", 100.123);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().BeEmpty();
			metric.Value.Serialize().Should().Be("count,delta=100.123");
			metric.Timestamp.Should().BeNull();
		}

		[Fact]
		public void TestDoubleCounterInvalidNumbers()
		{
			FluentActions.Invoking(() => MetricsFactory.CreateDoubleCounterDelta("mymetric", double.NegativeInfinity))
				.Should().Throw<MetricException>().WithMessage("Value is infinite.");

			FluentActions.Invoking(() => MetricsFactory.CreateDoubleCounterDelta("mymetric", -double.NegativeInfinity))
				.Should().Throw<MetricException>().WithMessage("Value is infinite.");

			FluentActions.Invoking(() => MetricsFactory.CreateDoubleCounterDelta("mymetric", double.NaN))
				.Should().Throw<MetricException>().WithMessage("Value is NaN.");
		}

		[Fact]
		public void TestDoubleGauge()
		{
			var metric = MetricsFactory.CreateDoubleGauge("mymetric", 123.456, TestDims);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(TestDims);
			metric.Value.Serialize().Should().Be("gauge,123.456");
			metric.Timestamp.Should().BeNull();
		}

		[Fact]
		public void TestDoubleGaugeTimestamp()
		{
			var metric = MetricsFactory.CreateDoubleGauge("mymetric", 123.456, TestDims, TestTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(TestDims);
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
			metric.Timestamp.Should().BeNull();
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
			var metric = MetricsFactory.CreateDoubleSummary("mymetric", 2.5, 5.7, 12.3, 4, TestDims);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(TestDims);
			metric.Value.Serialize().Should().Be("gauge,min=2.5,max=5.7,sum=12.3,count=4");
			metric.Timestamp.Should().BeNull();
		}

		[Fact]
		public void TestDoubleSummaryTimestamp()
		{
			var metric = MetricsFactory.CreateDoubleSummary("mymetric", 2.5, 5.7, 12.3, 4, TestDims, TestTimestamp);
			metric.MetricName.Should().Be("mymetric");
			metric.Dimensions.Should().Equal(TestDims);
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
			metric.Timestamp.Should().BeNull();
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
			var values = new List<double> {1.1, double.PositiveInfinity, double.NegativeInfinity, double.NaN};
			Func<double, bool> notNanOrInf = d => !double.IsInfinity(d) && !double.IsNaN(d);
			foreach (var i in values)
			{
				foreach (var j in values)
				{
					foreach (var k in values)
					{
						if (notNanOrInf(i) && notNanOrInf(j) && notNanOrInf(k))
						{
							// if all values are 1.1, this is a valid configuration. Therefore, check that its correctly serialized.
							var metric = MetricsFactory.CreateDoubleSummary("mymetric", i, j, k, 1);
							metric.Value.Serialize().Should().Be("gauge,min=1.1,max=1.1,sum=1.1,count=1");
							continue;
						}

						// any of the other configuration is invalid and should throw.
						FluentActions.Invoking(() => MetricsFactory.CreateDoubleSummary("mymetric", i, j, k, 1))
							.Should().Throw<MetricException>();
					}
				}
			}
		}
	}
}
