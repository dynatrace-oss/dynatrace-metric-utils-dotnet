using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace Dynatrace.MetricUtils.Example
{
	class Program
	{
		static void Main(string[] args)
		{
			ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
			{
				builder.SetMinimumLevel(LogLevel.Debug)
						.AddConsole();
			});
			var logger = loggerFactory.CreateLogger<DynatraceMetricSerializer>();

			DynatraceMetricSerializer serializer = new DynatraceMetricSerializer(logger);
			var dimensions = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("dim1", "val1") };
			Metric metric = MetricsFactory.CreateLongTotalCounter("counter", dimensions, 3L, DateTime.Now);
			Console.WriteLine(serializer.SerializeMetric(metric));
		}
	}
}
