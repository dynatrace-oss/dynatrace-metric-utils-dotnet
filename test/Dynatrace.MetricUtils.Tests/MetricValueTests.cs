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
using static Dynatrace.MetricUtils.MetricValue;

namespace Dynatrace.MetricUtils.Tests
{
	public class MetricValueTests
	{
		[Fact]
		public void TestLongCounterValue()
		{
			new LongCounterValue(100).Serialize().Should().Be("count,delta=100");
			new LongCounterValue(-10).Serialize().Should().Be("count,delta=-10");
			new LongCounterValue(long.MaxValue).Serialize().Should().Be("count,delta=9223372036854775807");
			new LongCounterValue(long.MinValue).Serialize().Should().Be("count,delta=-9223372036854775808");
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

			FluentActions.Invoking(() => new LongSummaryValue(1, 6, 10, -3)).Should().Throw<MetricException>()
				.WithMessage("Count cannot be less than 0.");
			FluentActions.Invoking(() => new LongSummaryValue(6, 1, 10, 5)).Should().Throw<MetricException>()
				.WithMessage("Min cannot be larger than max.");
		}

		[Fact]
		public void TestFormatDouble()
		{
			FormatDouble(0).Should().Be("0");
			FormatDouble(-0).Should().Be("0");
			FormatDouble(0.0).Should().Be("0");
			FormatDouble(-0.0).Should().Be("0");
			FormatDouble(.0000000000000).Should().Be("0");
			FormatDouble(-0.0000000000000).Should().Be("0");
			FormatDouble(123.456).Should().Be("123.456");
			FormatDouble(-123.456).Should().Be("-123.456");
			FormatDouble(1.0 / 3).Should().Be("0.3333333333333333");
			FormatDouble(double.MinValue).Should().Be("-1.7976931348623157E+308");
			FormatDouble(double.MaxValue).Should().Be("1.7976931348623157E+308");
			FormatDouble(1e100).Should().Be("1.0E+100");
			FormatDouble(1e-100).Should().Be("1.0E-100");
			FormatDouble(-1e100).Should().Be("-1.0E+100");
			FormatDouble(-1e-100).Should().Be("-1.0E-100");
			FormatDouble(1.234e100).Should().Be("1.234E+100");
			FormatDouble(1.234e-100).Should().Be("1.234E-100");
			FormatDouble(-1.234e100).Should().Be("-1.234E+100");
			FormatDouble(-1.234e-100).Should().Be("-1.234E-100");
			FormatDouble(1_000_000_000_000_000_000).Should().Be("1.0E+18");
			FormatDouble(-1_000_000_000_000_000_000).Should().Be("-1.0E+18");
			FormatDouble(0.000_000_000_000_000_001).Should().Be("1.0E-18");
			FormatDouble(-0.000_000_000_000_000_001).Should().Be("-1.0E-18");
			FormatDouble(1_234_000_000_000_000_000).Should().Be("1.234E+18");
			FormatDouble(-1_234_000_000_000_000_000).Should().Be("-1.234E+18");
			FormatDouble(0.000_000_000_000_000_001_234).Should().Be("1.234E-18");
			FormatDouble(-0.000_000_000_000_000_001_234).Should().Be("-1.234E-18");
			FormatDouble(1.1234567890123456789).Should().Be("1.1234567890123457");
			FormatDouble(-1.1234567890123456789).Should().Be("-1.1234567890123457");
			FormatDouble(200.00000000000).Should().Be("200");
			FormatDouble(-200.000000000000).Should().Be("-200");

			// these should never happen, as the MetricValue constructors should throw.
			FormatDouble(double.NegativeInfinity).Should().Be("-Infinity");
			FormatDouble(double.PositiveInfinity).Should().Be("Infinity");
			FormatDouble(double.NaN).Should().Be("NaN");
		}

		[Fact]
		public void TestDoubleCounterValue()
		{
			new DoubleCounterValue(0).Serialize().Should().Be("count,delta=0");
			new DoubleCounterValue(-0.0).Serialize().Should().Be("count,delta=0");
			new DoubleCounterValue(123.456).Serialize().Should().Be("count,delta=123.456");
			new DoubleCounterValue(-123.456).Serialize().Should().Be("count,delta=-123.456");
			new DoubleCounterValue(1.0 / 3).Serialize().Should().Be("count,delta=0.3333333333333333");
			new DoubleCounterValue(200.00000000000000).Serialize().Should().Be("count,delta=200");

			FluentActions.Invoking(() => new DoubleCounterValue(double.NegativeInfinity)).Should()
				.Throw<MetricException>().WithMessage("Value is infinite.");
			FluentActions.Invoking(() => new DoubleCounterValue(double.PositiveInfinity)).Should()
				.Throw<MetricException>().WithMessage("Value is infinite.");
			FluentActions.Invoking(() => new DoubleCounterValue(double.NaN)).Should().Throw<MetricException>()
				.WithMessage("Value is NaN.");
		}

		[Fact]
		public void TestDoubleGaugeValue()
		{
			new DoubleGaugeValue(0).Serialize().Should().Be("gauge,0");
			new DoubleGaugeValue(-0.0).Serialize().Should().Be("gauge,0");
			new DoubleGaugeValue(123.456).Serialize().Should().Be("gauge,123.456");
			new DoubleGaugeValue(-123.456).Serialize().Should().Be("gauge,-123.456");
			new DoubleGaugeValue(1.0 / 3).Serialize().Should().Be("gauge,0.3333333333333333");
			new DoubleGaugeValue(200.00000000000000).Serialize().Should().Be("gauge,200");

			FluentActions.Invoking(() => new DoubleGaugeValue(double.NegativeInfinity)).Should()
				.Throw<MetricException>().WithMessage("Value is infinite.");
			FluentActions.Invoking(() => new DoubleGaugeValue(double.PositiveInfinity)).Should()
				.Throw<MetricException>().WithMessage("Value is infinite.");
			FluentActions.Invoking(() => new DoubleGaugeValue(double.NaN)).Should().Throw<MetricException>()
				.WithMessage("Value is NaN.");
		}

		[Fact]
		public void TestDoubleSummaryValue()
		{
			new DoubleSummaryValue(1.2, 3.4, 8.9, 4).Serialize().Should().Be("gauge,min=1.2,max=3.4,sum=8.9,count=4");
			new DoubleSummaryValue(double.MinValue, double.MaxValue, double.MaxValue, 5).Serialize().Should().Be(
				"gauge,min=-1.7976931348623157E+308,max=1.7976931348623157E+308,sum=1.7976931348623157E+308,count=5");
			new DoubleSummaryValue(1.23e-18, 1.23e18, 5.6e18, 7).Serialize().Should()
				.Be("gauge,min=1.23E-18,max=1.23E+18,sum=5.6E+18,count=7");
			new DoubleSummaryValue(1.2, 1.2, 1.2, 4).Serialize().Should().Be("gauge,min=1.2,max=1.2,sum=1.2,count=4");

			FluentActions.Invoking(() => new DoubleSummaryValue(1.2, 2.3, 5.6, -3)).Should().Throw<MetricException>()
				.WithMessage("Count cannot be less than 0.");
			FluentActions.Invoking(() => new DoubleSummaryValue(6.5, 1.2, 10.7, 5)).Should().Throw<MetricException>()
				.WithMessage("Min cannot be larger than max.");
		}

		[Fact]
		public void TestDoubleSummaryValueInvalidsLoop()
		{
			var values = new List<double> {1.2, double.NegativeInfinity, double.PositiveInfinity, double.NaN};
			Func<double, bool> isValidDouble = d => !double.IsInfinity(d) && !double.IsNaN(d);
			foreach (var i in values)
			{
				foreach (var j in values)
				{
					foreach (var k in values)
					{
						if (isValidDouble(i) && isValidDouble(j) && isValidDouble(k))
						{
							new DoubleSummaryValue(i, j, k, 1).Serialize().Should()
								.Be("gauge,min=1.2,max=1.2,sum=1.2,count=1");
						}
						else
						{
							FluentActions.Invoking(() => new DoubleSummaryValue(i, j, k, 1)).Should()
								.Throw<MetricException>();
						}
					}
				}
			}
		}
	}
}
