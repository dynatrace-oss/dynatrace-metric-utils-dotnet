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

using Xunit;
using FluentAssertions;
using static Dynatrace.MetricUtils.MetricValue;

namespace Dynatrace.MetricUtils.Tests
{
	public class MetricValueTests
	{
		[Fact]
		public void TestLongCounterValueDelta()
		{
			new LongCounterValue(100, true).Serialize().Should().Be("count,delta=100");
			new LongCounterValue(-10, true).Serialize().Should().Be("count,delta=-10");
			new LongCounterValue(long.MaxValue, true).Serialize().Should().Be("count,delta=9223372036854775807");
			new LongCounterValue(long.MinValue, true).Serialize().Should().Be("count,delta=-9223372036854775808");
		}

		[Fact]
		public void TestLongCounterValueTotal()
		{
			new LongCounterValue(100, false).Serialize().Should().Be("count,100");
			new LongCounterValue(-10, false).Serialize().Should().Be("count,-10");
			new LongCounterValue(long.MaxValue, false).Serialize().Should().Be("count,9223372036854775807");
			new LongCounterValue(long.MinValue, false).Serialize().Should().Be("count,-9223372036854775808");
		}

		[Fact]
		public void TestLongGaugeValue()
		{
			new LongGaugeValue(100).Serialize().Should().Be("gauge,100");
			new LongGaugeValue(-10).Serialize().Should().Be("gauge,-10");
			new LongGaugeValue(long.MaxValue).Serialize().Should().Be("gauge,9223372036854775807");
			new LongGaugeValue(long.MinValue).Serialize().Should().Be("gauge,-9223372036854775808");
		}

		[Fact]
		public void TestLongSummaryValue()
		{
			new LongSummaryValue(1, 6, 10, 5).Serialize().Should().Be("gauge,min=1,max=6,sum=10,count=5");
			new LongSummaryValue(-5, -1, -10, 5).Serialize().Should().Be("gauge,min=-5,max=-1,sum=-10,count=5");
			new LongSummaryValue(1, 1, 1, 1).Serialize().Should().Be("gauge,min=1,max=1,sum=1,count=1");
			new LongSummaryValue(1, 6, 10, 0).Serialize().Should().Be("gauge,min=1,max=6,sum=10,count=0");

			FluentActions.Invoking(() => new LongSummaryValue(1, 6, 10, -3)).Should().Throw<MetricException>().WithMessage("Count cannot be less than 0.");
			FluentActions.Invoking(() => new LongSummaryValue(6, 1, 10, 5)).Should().Throw<MetricException>().WithMessage("Min cannot be larger than max.");
		}

		[Fact]
		public void TestDoubleCounterValueDelta()
		{
			// todo move these to FormatDouble
			new DoubleCounterValue(0, true).Serialize().Should().Be("count,delta=0");
			new DoubleCounterValue(0.0, true).Serialize().Should().Be("count,delta=0");
			new DoubleCounterValue(-0.0, true).Serialize().Should().Be("count,delta=0");
			new DoubleCounterValue(123.456, true).Serialize().Should().Be("count,delta=123.456");
			new DoubleCounterValue(-123.456, true).Serialize().Should().Be("count,delta=-123.456");
			new DoubleCounterValue(1.0/3, true).Serialize().Should().Be("count,delta=0.3333333333333333");
			new DoubleCounterValue(double.MinValue, true).Serialize().Should().Be("count,delta=-1.7976931348623157E+308");
			new DoubleCounterValue(double.MaxValue, true).Serialize().Should().Be("count,delta=1.7976931348623157E+308");
			new DoubleCounterValue(1.0e100, true).Serialize().Should().Be("count,delta=1E+100");
			new DoubleCounterValue(1.0e-100, true).Serialize().Should().Be("count,delta=1E-100");
			new DoubleCounterValue(1_000_000_000_000_000_000, true).Serialize().Should().Be("count,delta=1E+18");
			new DoubleCounterValue(-1_000_000_000_000_000_000, true).Serialize().Should().Be("count,delta=-1E+18");
			new DoubleCounterValue(0.000_000_000_000_000_001, true).Serialize().Should().Be("count,delta=1E-18");
			new DoubleCounterValue(-0.000_000_000_000_000_001, true).Serialize().Should().Be("count,delta=-1E-18");
			new DoubleCounterValue(1.1234567890123456789, true).Serialize().Should().Be("count,delta=1.1234567890123457");
			new DoubleCounterValue(-1.1234567890123456789, true).Serialize().Should().Be("count,delta=-1.1234567890123457");
			new DoubleCounterValue(200.00000000000000, true).Serialize().Should().Be("count,delta=200");

			FluentActions.Invoking(() => new DoubleCounterValue(double.NegativeInfinity, true)).Should().Throw<MetricException>().WithMessage("Value is infinite.");
			FluentActions.Invoking(() => new DoubleCounterValue(double.PositiveInfinity, true)).Should().Throw<MetricException>().WithMessage("Value is infinite.");
			FluentActions.Invoking(() => new DoubleCounterValue(double.NaN, true)).Should().Throw<MetricException>().WithMessage("Value is NaN.");
		}
	}
}
// TODO: test 1/3 and how many decimal places.
