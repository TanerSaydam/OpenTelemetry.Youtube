using System.Diagnostics.Metrics;

namespace Observability.Youtube.Metric.WebAPI;

public static class OpenTelemetryMetric
{
    public static int OrderCount { get; set; } = 0;
    public static readonly Meter meter = new("metic.meter.api");
    public static Counter<int> OrderCounter = meter.CreateCounter<int>("order_count_event");
    public static ObservableCounter<int> OrderObserableCounter = meter.CreateObservableCounter("order_observable-count_event", () => new Measurement<int>(OrderCount));
}
