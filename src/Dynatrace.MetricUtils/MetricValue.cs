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

		internal sealed class DoubleCounterValue: IMetricValue {
			private double value;
			private bool isDelta;

			public DoubleCounterValue(double value, bool isDelta)
			{
				// todo ensure that double counter value is not Nan or inf
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

	}

}