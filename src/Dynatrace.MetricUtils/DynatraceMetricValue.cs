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

namespace Dynatrace.MetricUtils
{
	/// <summary>Interface for the Metric values.</summary>
	public interface IDynatraceMetricValue
	{
		/// <summary>Transforms the <see cref="MetricValue" /> to a <see cref="string" />.</summary>
		/// <returns>The string representation for the <see cref="MetricValue" /></returns>
		string Serialize();
	}

	internal static class MetricValue
	{
		private static void ThrowOnNanOrInfDouble(double d)
		{
			if (double.IsNaN(d))
			{
				throw new DynatraceMetricException("Value is NaN.");
			}

			if (double.IsInfinity(d))
			{
				throw new DynatraceMetricException("Value is infinite.");
			}
		}

		private static void ThrowOnNanOrInfDoubles(params double[] doubles)
		{
			foreach (var d in doubles)
			{
				ThrowOnNanOrInfDouble(d);
			}
		}

		internal sealed class LongCounterValue : IDynatraceMetricValue
		{
			private readonly long _value;

			public LongCounterValue(long value) => _value = value;

			public string Serialize() => $"count,delta={_value}";
		}

		internal sealed class LongGaugeValue : IDynatraceMetricValue
		{
			private readonly long _value;

			public LongGaugeValue(long value) => _value = value;

			public string Serialize() => $"gauge,{_value}";
		}

		internal sealed class LongSummaryValue : IDynatraceMetricValue
		{
			private readonly long _count;
			private readonly long _max;
			private readonly long _min;
			private readonly long _sum;

			public LongSummaryValue(long min, long max, long sum, long count)
			{
				if (count < 0)
				{
					throw new DynatraceMetricException("Count cannot be less than 0.");
				}

				if (min > max)
				{
					throw new DynatraceMetricException("Min cannot be larger than max.");
				}

				_min = min;
				_max = max;
				_sum = sum;
				_count = count;
			}

			public string Serialize() => $"gauge,min={_min},max={_max},sum={_sum},count={_count}";
		}

		internal sealed class DoubleCounterValue : IDynatraceMetricValue
		{
			private readonly double _value;

			public DoubleCounterValue(double value)
			{
				ThrowOnNanOrInfDouble(value);
				_value = value;
			}

			public string Serialize() => $"count,delta={_value}";
		}

		internal sealed class DoubleGaugeValue : IDynatraceMetricValue
		{
			private readonly double _value;

			public DoubleGaugeValue(double value)
			{
				ThrowOnNanOrInfDouble(value);
				_value = value;
			}

			public string Serialize() => $"gauge,{_value}";
		}

		internal sealed class DoubleSummaryValue : IDynatraceMetricValue
		{
			private readonly long _count;
			private readonly double _max;
			private readonly double _min;
			private readonly double _sum;

			public DoubleSummaryValue(double min, double max, double sum, long count)
			{
				if (count < 0)
				{
					throw new DynatraceMetricException("Count cannot be less than 0.");
				}

				if (min > max)
				{
					throw new DynatraceMetricException("Min cannot be larger than max.");
				}

				ThrowOnNanOrInfDoubles(min, max, sum);

				_min = min;
				_max = max;
				_sum = sum;
				_count = count;
			}

			public string Serialize() =>
				$"gauge,min={_min},max={_max},sum={_sum},count={_count}";
		}
	}
}
