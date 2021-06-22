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
	public interface IMetricValue
	{
		string Serialize();
	}

	static class MetricValue
	{
		internal sealed class LongCounterValue : IMetricValue
		{
			private readonly long value;
			private readonly bool isDelta;
			public LongCounterValue(long value, bool isDelta)
			{
				this.value = value;
				this.isDelta = isDelta;
			}

			public string Serialize()
			{
				if (isDelta)
				{
					return $"count,delta={value}";
				}
				return $"count,{value}";
			}
		}

		internal sealed class LongGaugeValue : IMetricValue
		{
			private readonly long value;

			public LongGaugeValue(long value)
			{
				this.value = value;
			}

			public string Serialize()
			{
				return $"gauge,{value}";
			}
		}

		internal sealed class LongSummaryValue : IMetricValue
		{
			private readonly long min;
			private readonly long max;
			private readonly long sum;
			private readonly long count;

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

				this.min = min;
				this.max = max;
				this.sum = sum;
				this.count = count;
			}

			public string Serialize()
			{
				return $"gauge,min={min},max={max},sum={sum},count={count}";
			}
		}

		internal sealed class DoubleCounterValue : IMetricValue
		{
			private readonly double value;
			private readonly bool isDelta;

			public DoubleCounterValue(double value, bool isDelta)
			{
				ThrowOnNanOrInfDouble(value);
				this.value = value;
				this.isDelta = isDelta;
			}
			public string Serialize()
			{
				if (isDelta)
				{
					return $"count,delta={value}";
				}
				return $"count,{value}";
			}
		}

		internal sealed class DoubleGaugeValue : IMetricValue
		{
			private readonly double value;

			public DoubleGaugeValue(double value)
			{
				ThrowOnNanOrInfDouble(value);
				this.value = value;
			}

			public string Serialize()
			{
				return $"gauge,{value}";
			}
		}

		internal sealed class DoubleSummaryValue : IMetricValue
		{
			private readonly double min;
			private readonly double max;
			private readonly double sum;
			private readonly long count;

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

				this.min = min;
				this.max = max;
				this.sum = sum;
				this.count = count;
			}

			public string Serialize()
			{
				return $"gauge,min={min},max={max},sum={sum},count={count}";
			}
		}

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
	}
}
