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

namespace Dynatrace.MetricUtils
{
	public class Metric
	{
		internal Metric(string metricName, IEnumerable<KeyValuePair<string, string>> dimensions, IMetricValue value,
			DateTime? timestamp)
		{
			if (string.IsNullOrEmpty(metricName))
			{
				throw new MetricException("Metric name can't be null or empty.");
			}

			this.MetricName = metricName;
			this.Dimensions = dimensions;
			this.Value = value;
			this.Timestamp = timestamp;
		}

		public string MetricName { get; }
		public IEnumerable<KeyValuePair<string, string>> Dimensions { get; }
		public IMetricValue Value { get; }
		public DateTime? Timestamp { get; }
	}
}
