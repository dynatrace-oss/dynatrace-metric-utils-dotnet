using System;
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


        }
    }
}
