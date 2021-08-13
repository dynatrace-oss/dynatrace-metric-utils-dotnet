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
		[Theory]
		[InlineData(100, "count,delta=100")]
		[InlineData(-10, "count,delta=-10")]
		[InlineData(long.MaxValue, "count,delta=9223372036854775807")]
		[InlineData(long.MinValue, "count,delta=-9223372036854775808")]
		public void TestLongCounterValueDelta(long input, string expected) =>
			new LongCounterValue(input).Serialize().Should().Be(expected);


		[Theory]
		[InlineData(100, "gauge,100")]
		[InlineData(-10, "gauge,-10")]
		[InlineData(long.MaxValue, "gauge,9223372036854775807")]
		[InlineData(long.MinValue, "gauge,-9223372036854775808")]
		public void TestLongGaugeValue(long input, string expected) =>
			new LongGaugeValue(input).Serialize().Should().Be(expected);

		[Theory]
		[InlineData(1, 6, 10, 5, "gauge,min=1,max=6,sum=10,count=5")]
		[InlineData(-5, -1, -10, 5, "gauge,min=-5,max=-1,sum=-10,count=5")]
		[InlineData(1, 1, 1, 1, "gauge,min=1,max=1,sum=1,count=1")]
		[InlineData(1, 6, 10, 0, "gauge,min=1,max=6,sum=10,count=0")]
		public void TestLongSummaryValue(long min, long max, long sum, long count, string expected) =>
			new LongSummaryValue(min, max, sum, count).Serialize().Should().Be(expected);

		[Fact]
		public void TestInvalidValuesLongSummaryValue()
		{
			FluentActions.Invoking(() => new LongSummaryValue(1, 6, 10, -3)).Should().Throw<MetricException>()
				.WithMessage("Count cannot be less than 0.");
			FluentActions.Invoking(() => new LongSummaryValue(6, 1, 10, 5)).Should().Throw<MetricException>()
				.WithMessage("Min cannot be larger than max.");
		}

		[Theory]
		[InlineData(0, "0")]
		[InlineData(-0, "0")]
		[InlineData(0.0, "0")]
		[InlineData(-0.0, "0")]
		[InlineData(.0000000000000, "0")]
		[InlineData(-0.0000000000000, "0")]
		[InlineData(123.456, "123.456")]
		[InlineData(-123.456, "-123.456")]
		[InlineData(1.0 / 3, "0.333333333333333")]
		[InlineData(double.MinValue, "-1.79769313486232E+308")]
		[InlineData(double.MaxValue, "1.79769313486232E+308")]
		[InlineData(1e100, "1.0E+100")]
		[InlineData(1e-100, "1.0E-100")]
		[InlineData(-1e100, "-1.0E+100")]
		[InlineData(-1e-100, "-1.0E-100")]
		[InlineData(1.234e100, "1.234E+100")]
		[InlineData(1.234e-100, "1.234E-100")]
		[InlineData(-1.234e100, "-1.234E+100")]
		[InlineData(-1.234e-100, "-1.234E-100")]
		[InlineData(1_000_000_000_000_000_000, "1.0E+18")]
		[InlineData(-1_000_000_000_000_000_000, "-1.0E+18")]
		[InlineData(1_100_000_000_000_000_000, "1.1E+18")]
		[InlineData(-1_100_000_000_000_000_000, "-1.1E+18")]
		[InlineData(0.000_000_000_000_000_001, "1.0E-18")]
		[InlineData(-0.000_000_000_000_000_001, "-1.0E-18")]
		[InlineData(1_234_000_000_000_000_000, "1.234E+18")]
		[InlineData(-1_234_000_000_000_000_000, "-1.234E+18")]
		[InlineData(0.000_000_000_000_000_001_234, "1.234E-18")]
		[InlineData(-0.000_000_000_000_000_001_234, "-1.234E-18")]
		[InlineData(1.1234567890123456789, "1.12345678901235")]
		[InlineData(-1.1234567890123456789, "-1.12345678901235")]
		[InlineData(200.00000000000, "200")]
		[InlineData(-200.000000000000, "-200")]
		// these should never happen, as the MetricValue constructors should throw.
		[InlineData(double.NegativeInfinity, "-Infinity")]
		[InlineData(double.PositiveInfinity, "Infinity")]
		[InlineData(double.NaN, "NaN")]
		public void TestFormatDouble(double input, string expected) => FormatDouble(input).Should().Be(expected);

		[Theory]
		[InlineData(0, "count,delta=0")]
		[InlineData(-0.0, "count,delta=0")]
		[InlineData(123.456, "count,delta=123.456")]
		[InlineData(-123.456, "count,delta=-123.456")]
		[InlineData(1.0 / 3, "count,delta=0.333333333333333")]
		[InlineData(200.00000000000000, "count,delta=200")]
		public void TestDoubleCounterValueDelta(double input, string expected) =>
			new DoubleCounterValue(input).Serialize().Should().Be(expected);

		[Theory]
		[InlineData(double.NegativeInfinity, "Value is infinite.")]
		[InlineData(double.PositiveInfinity, "Value is infinite.")]
		[InlineData(double.NaN, "Value is NaN.")]
		public void TestDoubleCounterValueDeltaInvalid(double input, string expectedError) =>
			FluentActions.Invoking(() => new DoubleCounterValue(input)).Should()
				.Throw<MetricException>().WithMessage(expectedError);

		[Theory]
		[InlineData(0, "gauge,0")]
		[InlineData(-0.0, "gauge,0")]
		[InlineData(123.456, "gauge,123.456")]
		[InlineData(-123.456, "gauge,-123.456")]
		[InlineData(1.0 / 3, "gauge,0.333333333333333")]
		[InlineData(200.00000000000000, "gauge,200")]
		public void TestDoubleGaugeValue(double input, string expected) =>
			new DoubleGaugeValue(input).Serialize().Should().Be(expected);

		[Theory]
		[InlineData(double.NegativeInfinity, "Value is infinite.")]
		[InlineData(double.PositiveInfinity, "Value is infinite.")]
		[InlineData(double.NaN, "Value is NaN.")]
		public void TestDoubleGaugeValueInvalid(double input, string expectedError) =>
			FluentActions.Invoking(() => new DoubleGaugeValue(input)).Should()
				.Throw<MetricException>().WithMessage(expectedError);

		[Theory]
		[InlineData(1.2, 3.4, 8.9, 4, "gauge,min=1.2,max=3.4,sum=8.9,count=4")]
		[InlineData(double.MinValue, double.MaxValue, double.MaxValue, 5,
			"gauge,min=-1.79769313486232E+308,max=1.79769313486232E+308,sum=1.79769313486232E+308,count=5")]
		[InlineData(1.23e-18, 1.23e18, 5.6e18, 7, "gauge,min=1.23E-18,max=1.23E+18,sum=5.6E+18,count=7")]
		[InlineData(1.2, 1.2, 1.2, 4, "gauge,min=1.2,max=1.2,sum=1.2,count=4")]
		public void TestDoubleSummaryValue(double min, double max, double sum, long count, string expected) =>
			new DoubleSummaryValue(min, max, sum, count).Serialize().Should().Be(expected);

		[Fact]
		public void TestDoubleSummaryValueInvlid()
		{
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
