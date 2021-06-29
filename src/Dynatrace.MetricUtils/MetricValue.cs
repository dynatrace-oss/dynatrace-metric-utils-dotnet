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
using System.Globalization;

namespace Dynatrace.MetricUtils
{
	public interface IMetricValue
	{
		string Serialize();
	}

	internal static class MetricValue
	{
		private static void ThrowOnNanOrInfDouble(double d)
		{
			if (double.IsNaN(d))
			{
				throw new MetricException("Value is NaN.");
			}

			if (double.IsInfinity(d))
			{
				throw new MetricException("Value is infinite.");
			}
		}

		private static void ThrowOnNanOrInfDoubles(params double[] doubles)
		{
			foreach (var d in doubles)
			{
				ThrowOnNanOrInfDouble(d);
			}
		}

		internal static string FormatDouble(double d)
		{
			// d is exactly 0 or -0. Used to make sure -0 is exported as "0".
			if (Math.Abs(d).Equals(0.0))
			{
				return "0";
			}

			// use exponential notation with 15 decimal places and at least one trailing decimal place before the exponent.
			// for numbers greater than -10^-15 and smaller than 10^-15.
			if (Math.Abs(d) < 1e-15) {
				return d.ToString("0.0##############E-0", CultureInfo.InvariantCulture);
			}

			// for numbers greater than 10^15 or smaller than -10^15
			if (Math.Abs(d) > 1e+15) {
				return d.ToString("0.0##############E+0", CultureInfo.InvariantCulture);
			}

			// set a fixed number of decimal places, as different c# versions seem to have different defaults.
			var serialized = d.ToString("0.###############", CultureInfo.InvariantCulture);

			return serialized;
		}

		internal sealed class LongCounterValue : IMetricValue
		{
			private readonly long _value;

			public LongCounterValue(long value)
			{
				this._value = value;
			}

			public string Serialize()
			{
				return $"count,delta={this._value}";
			}
		}

		internal sealed class LongGaugeValue : IMetricValue
		{
			private readonly long _value;

			public LongGaugeValue(long value)
			{
				this._value = value;
			}

			public string Serialize()
			{
				return $"gauge,{this._value}";
			}
		}

		internal sealed class LongSummaryValue : IMetricValue
		{
			private readonly long _count;
			private readonly long _max;
			private readonly long _min;
			private readonly long _sum;

			public LongSummaryValue(long min, long max, long sum, long count)
			{
				if (count < 0)
				{
					throw new MetricException("Count cannot be less than 0.");
				}

				if (min > max)
				{
					throw new MetricException("Min cannot be larger than max.");
				}

				this._min = min;
				this._max = max;
				this._sum = sum;
				this._count = count;
			}

			public string Serialize()
			{
				return $"gauge,min={this._min},max={this._max},sum={this._sum},count={this._count}";
			}
		}

		internal sealed class DoubleCounterValue : IMetricValue
		{
			private readonly double _value;

			public DoubleCounterValue(double value)
			{
				ThrowOnNanOrInfDouble(value);
				this._value = value;
			}

			public string Serialize()
			{
				return $"count,delta={FormatDouble(this._value)}";
			}
		}

		internal sealed class DoubleGaugeValue : IMetricValue
		{
			private readonly double _value;

			public DoubleGaugeValue(double value)
			{
				ThrowOnNanOrInfDouble(value);
				this._value = value;
			}

			public string Serialize()
			{
				return $"gauge,{FormatDouble(this._value)}";
			}
		}

		internal sealed class DoubleSummaryValue : IMetricValue
		{
			private readonly long _count;
			private readonly double _max;
			private readonly double _min;
			private readonly double _sum;

			public DoubleSummaryValue(double min, double max, double sum, long count)
			{
				if (count < 0)
				{
					throw new MetricException("Count cannot be less than 0.");
				}

				if (min > max)
				{
					throw new MetricException("Min cannot be larger than max.");
				}

				ThrowOnNanOrInfDoubles(min, max, sum);

				this._min = min;
				this._max = max;
				this._sum = sum;
				this._count = count;
			}

			public string Serialize()
			{
				return
					$"gauge,min={FormatDouble(this._min)},max={FormatDouble(this._max)},sum={FormatDouble(this._sum)},count={FormatDouble(this._count)}";
			}
		}
	}
}
