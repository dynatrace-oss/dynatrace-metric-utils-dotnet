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
This repository also contains [an example project](src/Dynatrace.MetricUtils.Example) demonstrating the use of the `MetricsFactory` and the `MetricsSerializer`.

### Metric Creation

To create metrics, call one of the static methods on the `MetricsFactory`.
Available instruments are:

- Counter: `CreateLongCounterDelta` / `CreateDoubleCounterDelta`
- Gauge: `CreateLongGauge` / `CreateDoubleGauge`
- Summary: `CreateLongSummary` / `CreateDoubleSummary`

In the simplest form, metric creation looks like this:

```csharp
var metric = MetricsFactory.CreateLongCounterDelta("long-counter", 23);
```

Additionally, it is possible to pass a list of dimensions to the metric upon creation:

```csharp
var dimensions = new List<KeyValuePair<string, string>> {
  new KeyValuePair<string, string>("dim1", "val1"),
  new KeyValuePair<string, string>("dim2", "val2")
};

var metric = MetricsFactory.CreateLongCounterDelta("long-counter", 23, dimensions);
```

The dimensions will be added to the serialized metric.
See [the section on dimension precedence](#dimension-precedence) for more information.

Finally, it is also possible to add a timestamp to the metric:

```csharp
// Passing null for the dimensions will not add any dimensions to the metric.
// Of course it is possible to pass dimensions and a timestamp at the same time.
var metric = MetricsFactory.CreateLongCounterDelta("long-counter", 23, null, DateTime.Now))

// Alternatively, the dimensions parameter can be skipped and timestamp can be passed as a named parameter.
var metric = MetricsFactory.CreateLongCounterDelta("long-counter", 23, timestamp: new DateTime(2021, 01, 01, 12, 00, 00))
```

Timestamps that are before the year 2000 or after the year 3000 will be discarded.
In that case, metrics will be serialized without a timestamp.

If the metric timestamp is omitted or outside the range, the server timestamp is used upon ingestion.

### Metric serialization

The created metrics can then be serialized using a `MetricsSerializer`:

```csharp
// The logger is optional, but not providing one will result in all log messages being discarded:
var loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.Debug).AddConsole());
var logger = loggerFactory.CreateLogger<MetricsSerializer>();

// Create a list of default dimensions which are added to all metrics serialized by this serializer.
var defaultDimensions = new List<KeyValuePair<string, string>> {
  new KeyValuePair<string, string>("default1", "value1"),
  new KeyValuePair<string, string>("default2", "value2")
};

// Set up the MetricsSerializer. All parameters are optional:
var serializer = new MetricsSerializer(
  logger,             // This logger will be used to log errors in normalization.
  "prefix",           // A prefix added to all exported metric names.
  defaultDimensions,  // Default dimensions that will be added to all exported metrics.
  "metrics-source",   // Set the metrics source. Will be exported as dimension with "dt.metrics.source" as its key.
  true                // Turn Dynatrace metadata enrichment on or off (true by default).
);

var metric = MetricsFactory.CreateLongCounterDelta("long-counter", 23, timestamp: new DateTime(2021, 01, 01, 12, 00, 00));

// Serialize the metric
Console.WriteLine(serializer.SerializeMetric(metric));

// Should result in a line like: 
// prefix.long-counter,default1=value1,default2=value2,dt.metrics.source=metrics-source count,delta=23 1609502400000
```

### Common constants

The constants can be accessed via the static getter methods on the `DynatraceMetricApiConstants` class:

```csharp
Console.WriteLine(DynatraceMetricApiConstants.DefaultOneAgentEndpoint);
```

The use of the common constants is also shown in [the example project](src/Dynatrace.MetricUtils.Example)

Currently available constants are:

- the default [local OneAgent metric API endpoint](https://www.dynatrace.com/support/help/how-to-use-dynatrace/metrics/metric-ingestion/ingestion-methods/local-api/) (`DefaultOneAgentEndpoint`)
- the limit for how many metric lines can be ingested in one request (`PayloadLinesLimit`)
- the limit for how many dimensions can be added to each metric key (`MaximumDimensions`)

### Dynatrace Metadata enrichment

If the `enrichWithDynatraceMetadata` toggle in the `MetricsSerializer` constructor is set to `true`, an attempt is made to read Dynatrace metadata.
On a host with a running OneAgent, setting this option will collect metadata and add them as dimensions to all serialized metrics.
Metadata typically consist of the Dynatrace host ID and process group ID.
More information on the underlying feature that is used by the library can be found in the [Dynatrace documentation](https://www.dynatrace.com/support/help/how-to-use-dynatrace/metrics/metric-ingestion/ingestion-methods/enrich-metrics/).

### Dimension precedence

Since there are multiple levels of dimensions (default, metric-specific, serializer-specific) and duplicate keys are not allowed, there is a specified precedence in dimension keys.
Default dimensions will be overwritten by metric-specific dimensions, and all dimensions will be overwritten by serializer-specific dimensions if they share the same key after normalization.
Serializer-specific dimensions include the [metadata dimensions](#dynatrace-metadata-enrichment), as well as the metrics source, which is added as a dimension with `dt.metrics.source` as its key.
Note that the serializer-specific dimensions will only contain [dimension keys reserved by Dynatrace](https://www.dynatrace.com/support/help/how-to-use-dynatrace/metrics/metric-ingestion/metric-ingestion-protocol/#syntax).
