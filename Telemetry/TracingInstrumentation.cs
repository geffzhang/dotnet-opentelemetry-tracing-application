using System.Diagnostics;

namespace TracingDemo.Telemetry;

public static class TracingInstrumentation
{
    public const string ServiceName = "OrderProcessingService";
    public static readonly ActivitySource ActivitySource = new(ServiceName, "1.0.0");
}
