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
using Microsoft.Extensions.Logging;

namespace Dynatrace.MetricUtils.Example
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			// Create a logger for the DynatraceMetricsSerializer. In this case, log to the console.
			var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug).AddConsole());
			var logger = loggerFactory.CreateLogger<DynatraceMetricsSerializer>();

			// Set up default dimensions, which will be added to every serialized metric.
			var defaultDimensions =
				new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>("default1", "value1"),
					new KeyValuePair<string, string>("default2", "value2")
				};
			// Set up a DynatraceMetricsSerializer. All parameters are optional.
			// If no logger is provided, log information is discarded.
			// the serializer is intended to be used for many metrics.
			var serializer = new DynatraceMetricsSerializer(logger, "prefix", defaultDimensions);

			// then, create metrics themselves using the DynatraceMetricsFactory
			var metricDimensions =
				new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("dim1", "val1") };
			var metrics = new List<DynatraceMetric>
			{
				// Specify the current time to add a timestamp to the metric line
				DynatraceMetricsFactory.CreateLongCounterDelta("long-counter", 23, metricDimensions, DateTimeOffset.UtcNow),
				// But it is also possible to specify the DateTimeOffset as a specific time explicitly:
				DynatraceMetricsFactory.CreateLongGauge("long-gauge", 34, metricDimensions,
					new DateTimeOffset(2021, 01, 01, 12, 00, 00, TimeSpan.Zero)),
				// summary values combine min, max, sum, and count.
				DynatraceMetricsFactory.CreateLongSummary("long-summary", 3, 5, 18, 4, metricDimensions),
				// dimensions are optional.
				DynatraceMetricsFactory.CreateDoubleCounterDelta("double-summary", 3.1415),
				DynatraceMetricsFactory.CreateDoubleGauge("double-gauge", 4.567, metricDimensions),
				DynatraceMetricsFactory.CreateDoubleSummary("double-summary", 3.1, 6.543, 20.123, 4, metricDimensions)
			};

			// Finally, serialize the metrics to strings:
			foreach (var metric in metrics)
			{
				Console.WriteLine(serializer.SerializeMetric(metric));
			}

			// Will produce the following output:
			// prefix.long-counter,default1=value1,default2=value2,dim1=val1 count,delta=23 1625062552272
			// prefix.long-gauge,default1=value1,default2=value2,dim1=val1 gauge,34 1609502400000
			// prefix.long-summary,default1=value1,default2=value2,dim1=val1 gauge,min=3,max=5,sum=18,count=4
			// prefix.double-summary,default1=value1,default2=value2 count,delta=3.1415
			// prefix.double-gauge,default1=value1,default2=value2,dim1=val1 gauge,4.567
			// prefix.double-summary,default1=value1,default2=value2,dim1=val1 gauge,min=3.1,max=6.543,sum=20.123,count=4

			// DynatraceMetricApiConstants contains constants that can be accessed like this:
			Console.WriteLine(
				$"Default local OneAgent endpoint:\t{DynatraceMetricApiConstants.DefaultOneAgentEndpoint}");
			Console.WriteLine($"Maximum lines per ingest request:\t{DynatraceMetricApiConstants.PayloadLinesLimit}");

			try
			{
				// an empty metric name is not permitted
				DynatraceMetricsFactory.CreateLongGauge("", 2);
			}
			catch (DynatraceMetricException me)
			{
				Console.WriteLine(me.Message);
			}

			// Upon creation of invalid metrics, a DynatraceMetricException is thrown:
			try
			{
				// min > max is an invalid state and will throw.
				DynatraceMetricsFactory.CreateLongSummary("metric", 100, 10, 10, 3);
			}
			catch (DynatraceMetricException me)
			{
				Console.WriteLine(me.Message);
			}
		}
	}
}
