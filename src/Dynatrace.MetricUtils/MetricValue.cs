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

		internal sealed class LongCounterValue : IMetricValue
		{
			private readonly bool _isDelta;
			private readonly long _value;

			public LongCounterValue(long value, bool isDelta)
			{
				this._value = value;
				this._isDelta = isDelta;
			}

			public string Serialize()
			{
				if (this._isDelta)
				{
					return $"count,delta={this._value}";
				}

				return $"count,{this._value}";
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
			private readonly bool _isDelta;
			private readonly double _value;

			public DoubleCounterValue(double value, bool isDelta)
			{
				ThrowOnNanOrInfDouble(value);
				this._value = value;
				this._isDelta = isDelta;
			}

			public string Serialize()
			{
				if (this._isDelta)
				{
					return $"count,delta={this._value}";
				}

				return $"count,{this._value}";
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
				return $"gauge,{this._value}";
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
				return $"gauge,min={this._min},max={this._max},sum={this._sum},count={this._count}";
			}
		}
	}
}
