﻿// <copyright company="Dynatrace LLC">
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
using Microsoft.Extensions.Logging;

namespace Dynatrace.MetricUtils.Example
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			// Create a logger for the MetricsSerializer. In this case, log to the console.
			var loggerFactory = LoggerFactory.Create(builder =>
			{
				builder.SetMinimumLevel(LogLevel.Debug)
					.AddConsole();
			});
			var logger = loggerFactory.CreateLogger<MetricsSerializer>();

			// Set up default dimensions, which will be added to every serialized metric.
			var defaultDimensions =
				new List<KeyValuePair<string, string>> {new("default1", "value1"), new("default2", "value2")};
			// Set up a Metrics Serializer. All parameters are optional.
			// If no logger is provided, log information is discarded.
			// the serializer is intended to be used for many metrics.
			var serializer = new MetricsSerializer(logger, "prefix", defaultDimensions);

			// then, create metrics themselves using the MetricsFactory
			var metricDimensions = new List<KeyValuePair<string, string>> {new("dim1", "val1")};
			var metrics = new List<Metric>
			{
				// If no DateTime is specified as the last parameter, the current timestamp will be used
				MetricsFactory.CreateLongCounter("long-counter", 23, metricDimensions),
				// But it is also possible to specify the DateTime explicitly:
				MetricsFactory.CreateLongGauge("long-gauge", 34, metricDimensions,
					new DateTime(2021, 01, 01, 12, 00, 00)),
				// summary values combine min, max, sum, and count.
				MetricsFactory.CreateLongSummary("long-summary", 3, 5, 18, 4, metricDimensions),
				// dimensions are also optional.
				MetricsFactory.CreateDoubleCounter("double-summary", 3.1415),
				MetricsFactory.CreateDoubleGauge("double-gauge", 4.567, metricDimensions),
				MetricsFactory.CreateDoubleSummary("double-summary", 3.1, 6.543, 20.123, 4, metricDimensions)
			};

			// Finally, serialize the metrics to strings:
			foreach (var metric in metrics)
			{
				Console.WriteLine(serializer.SerializeMetric(metric));
			}

			// Will produce the following output:
			// prefix.long-counter,default1=value1,default2=value2,dim1=val1 count,delta=23 1624544148084
			// prefix.long-gauge,default1=value1,default2=value2,dim1=val1 gauge,34 1609502400000
			// prefix.long-summary,default1=value1,default2=value2,dim1=val1 gauge,min=3,max=5,sum=18,count=4 1624544148088
			// prefix.double-summary,default1=value1,default2=value2 count,delta=3.1415 1624544148089
			// prefix.double-gauge,default1=value1,default2=value2,dim1=val1 gauge,4.567 1624544148089
			// prefix.double-summary,default1=value1,default2=value2,dim1=val1 gauge,min=3.1,max=6.543,sum=20.123,count=4 1624544148089
		}
	}
}
