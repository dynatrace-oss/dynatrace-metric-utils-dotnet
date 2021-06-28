# dynatrace-metric-utils-dotnet

.NET utility for interacting with the [Dynatrace v2 metrics API](https://www.dynatrace.com/support/help/dynatrace-api/environment-api/metric-v2/).

## Installation

Run the following command while in the project directory to add the NuGet package to your project:
<!-- TODO Add link to the nuget package -->

```sh
dotnet add package Dynatrace.MetricUtils
```

## Usage

Using this library to create metric lines is a two-step process.
First, create lines using the `MetricsFactory`.
Then, serialize them using a `MetricsSerializer`.
Furthermore, this library contains static constants for use in projects consuming this library.

### Metric Creation

To create metrics, call one of the static methods on the `MetricsFactory`.
Available instruments are:

- Counter: `CreateLongCounter` / `CreateDoubleCounter`
- Gauge: `CreateLongGauge` / `CreateDoubleGauge`
- Summary: `CreateLongSummary` / `CreateDoubleSummary`

In the simplest form, metric creation looks like this:

```csharp
var metric = MetricsFactory.CreateLongCounter("long-counter", 23);
```

This will create a metric with the current timestamp.

Additionally, it is possible to pass a list of dimensions to the metric upon creation:

```csharp

var dimensions = new List<KeyValuePair<string, string>> {new("dim1", "val1"), new("dim2", "val2")};

var metric = MetricsFactory.CreateLongCounter("long-counter", 23, dimensions);
```

The dimensions will be added to the serialized metric.
See [the section on dimension precedence](#dimension-precedence) for more information.

Finally, it is also possible to add a custom timestamp to the metric:

```csharp
// Passing null for the dimensions will not add any dimensions to the metric.
var metric = MetricsFactory.CreateLongCounter("long-counter", 23, null, new DateTime(2021, 01, 01, 12, 00, 00))

// Alternatively, the dimensions parameter can be skipped and timestamp can be passed as a named parameter.
var metric = MetricsFactory.CreateLongCounter("long-counter", 23, timestamp: new DateTime(2021, 01, 01, 12, 00, 00))
```

Timestamps that are before the year 2000 or after the year 3000 will be discarded.
In that case, metrics will be serialized without a timestamp, and the server timestamp is used upon ingestion.

### Metric serialization

The created metrics can then be serialized using a `MetricsSerializer`:

```csharp
// The logger is optional, but not providing one will result in all log messages being discarded:
var loggerFactory = LoggerFactory.Create(builder =>
{
  builder.SetMinimumLevel(LogLevel.Debug).AddConsole();
});
var logger = loggerFactory.CreateLogger<MetricsSerializer>()

// Create a list of default dimensions which are added to all metrics serialized by this serializer.
var defaultDimensions = new List<KeyValuePair<string, string>> {new("default1", "value1"), new("default2", "value2")};

// Set up the MetricsSerializer. All parameters are optional:
var serializer = new MetricsSerializer(
  logger,    // This logger will be used to log errors in normalization.
  "prefix",   // A prefix added to all exported metric names.
  defaultDimensions, // Default dimensions that will be added to all exported metrics.
  "metrics-source",  // Set the metrics source. Will be exported as dimension with "dt.metrics.source" as its key.
  true    // Turn Dynatrace metadata enrichment on or off (true by default).
);

// Serialize the metric
Console.WriteLine(serializer.SerializeMetric(metric));

// Should result in a line like: 
// prefix.long-counter,default1=value1,default2=value2,dim1=val1 count,delta=23 1624622933154
```

### Common constants

The constants can be accessed via the static getter methods:

```csharp
Console.WriteLine(DynatraceMetricApiConstants.DefaultOneAgentEndpoint);
```

This repository contains [an example project](src/Dynatrace.MetricUtils.Example) demonstrating the use of the `MetricsFactory` and the `MetricsSerializer`.
It also shows how to access the common constants.

Currently available constants are:

- the default [local OneAgent metric API](https://www.dynatrace.com/support/help/how-to-use-dynatrace/metrics/metric-ingestion/ingestion-methods/local-api/) endpoint (`DynatraceMetricApiConstants.DefaultOneAgentEndpoint`)
- the limit for how many metric lines can be ingested in one request (`DynatraceMetricApiConstants.PayloadLinesLimit`)
- the limit for how many dimensions can be added to each metric key (`DynatraceMetricApiConstants.MaximumDimensions`)

### Dynatrace Metadata enrichment

If the `enrichWithDynatraceMetadata` toggle in the `MetricsSerializer` constructor is set to `true`, an attempt is made to read Dynatrace metadata.
On a host with a running OneAgent, setting this option will collect metadata and add them as dimensions to all serialized metrics.
Metadata typically consist of the Dynatrace host ID and process group ID.
More information on the underlying feature can be found in the [Dynatrace documentation](https://www.dynatrace.com/support/help/how-to-use-dynatrace/metrics/metric-ingestion/ingestion-methods/enrich-metrics/).

### Dimension precedence

Since there are multiple levels of dimensions (default, metric-specfic, serializer-specific) and duplicate keys are not allowed, there is a specified precedence in dimension keys.
Default dimensions will be overwritten by metric-specific dimensions, and all dimensions will be overwritten by serializer-specific dimensions if they share the same key after normalization.
Serializer-specific dimensions include the [metadata dimensions](#dynatrace-metadata-enrichment), as well as the metrics source, which is added as a dimension with `dt.metrics.source` as its key.
Note that the serializer-specific dimensions will only contain [dimension keys reserved by Dynatrace](https://www.dynatrace.com/support/help/how-to-use-dynatrace/metrics/metric-ingestion/metric-ingestion-protocol/#syntax).
