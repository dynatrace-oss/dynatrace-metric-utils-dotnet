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
	/// <summary>
	///     Interface for the Metric values.
	/// </summary>
	public interface IMetricValue
	{
		/// <summary>
		///     Transforms the <see cref="MetricValue" /> to a <see cref="string" />.
		/// </summary>
		/// <returns>The string representation for the <see cref="MetricValue" /></returns>
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

		/// <remarks>
		///     Numbers with an absolute value smaller than 10^-15 (except 0) are serialized in exponential notation.
		///     Numbers with an absolute value larger than 10^15 are serialized in exponential notation.
		///     All other numbers are serialized with a maximum of 15 decimal places.
		///     This is specified due to doubles being serialized with a different number of decimal places
		///     depending on the used .NET core version when using the generic double.ToString method,
		///     This discrepancy would break the unit tests and is therefore explicitly specified.
		/// </remarks>
		internal static string FormatDouble(double d)
		{
			// d is exactly 0 or -0. Used to make sure -0 is exported as "0".
			if (Math.Abs(d).Equals(0.0))
			{
				return "0";
			}

			// use exponential notation with 15 decimal places and at least one trailing decimal place before the exponent.
			// for numbers greater than -10^-15 and smaller than 10^-15.
			if (Math.Abs(d) < 1e-15)
			{
				return d.ToString("0.0##############E-0", CultureInfo.InvariantCulture);
			}

			// for numbers greater than 10^15 or smaller than -10^15
			if (Math.Abs(d) > 1e+15)
			{
				return d.ToString("0.0##############E+0", CultureInfo.InvariantCulture);
			}

			// set a fixed number of decimal places, as different .net versions seem to have different defaults.
			var serialized = d.ToString("0.###############", CultureInfo.InvariantCulture);

			return serialized;
		}

		internal sealed class LongCounterValue : IMetricValue
		{
			private readonly bool _isDelta;
			private readonly long _value;

			public LongCounterValue(long value, bool isDelta = true)
			{
				_value = value;
				_isDelta = isDelta;
			}

			public string Serialize()
			{
				if (_isDelta)
				{
					return $"count,delta={_value}";
				}

				return $"count,{_value}";
			}
		}

		internal sealed class LongGaugeValue : IMetricValue
		{
			private readonly long _value;

			public LongGaugeValue(long value) => _value = value;

			public string Serialize() => $"gauge,{_value}";
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

				_min = min;
				_max = max;
				_sum = sum;
				_count = count;
			}

			public string Serialize() => $"gauge,min={_min},max={_max},sum={_sum},count={_count}";
		}

		internal sealed class DoubleCounterValue : IMetricValue
		{
			private readonly bool _isDelta;
			private readonly double _value;

			public DoubleCounterValue(double value, bool isDelta = true)
			{
				ThrowOnNanOrInfDouble(value);
				_value = value;
				_isDelta = isDelta;
			}

			public string Serialize()
			{
				if (_isDelta)
				{
					return $"count,delta={FormatDouble(_value)}";
				}

				return $"count,{FormatDouble(_value)}";
			}
		}

		internal sealed class DoubleGaugeValue : IMetricValue
		{
			private readonly double _value;

			public DoubleGaugeValue(double value)
			{
				ThrowOnNanOrInfDouble(value);
				_value = value;
			}

			public string Serialize() => $"gauge,{FormatDouble(_value)}";
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

				_min = min;
				_max = max;
				_sum = sum;
				_count = count;
			}

			public string Serialize() =>
				$"gauge,min={FormatDouble(_min)},max={FormatDouble(_max)},sum={FormatDouble(_sum)},count={FormatDouble(_count)}";
		}
	}
}
