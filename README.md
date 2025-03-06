# .NET Sample Application with OpenTelemetry Tracing

A sample .NET Web API application demonstrating distributed tracing with OpenTelemetry. The application implements a simple order processing system where you can create and retrieve orders, with all operations being traced and monitored with OpenObserve.

## About the Demo Application

This demo implements two main endpoints:
- `POST /order` - Create a new order
- `GET /order/{id}` - Retrieve an order by ID

Each endpoint is instrumented with OpenTelemetry to capture:
- HTTP request details
- Order processing operations
- External service calls
- Error scenarios

## Prerequisites

- [.NET SDK 6.0 or newer](https://dotnet.microsoft.com/download)
- OpenObserve: You can get started with [OpenObserve Cloud](https://cloud.openobserve.ai) or a [self hosted installation](https://openobserve.ai/docs/quickstart/#self-hosted-installation). 

## Getting Started

1. **Clone the Repository:**
```bash
git clone https://github.com/openobserve/dotnet-opentelemetry-tracing-application
cd dotnet-opentelemetry-tracing-application
```

2. **Add Required Packages:**
```bash
# Add core OpenTelemetry packages
dotnet add package OpenTelemetry --version 1.7.0
dotnet add package OpenTelemetry.Extensions.Hosting --version 1.7.0
dotnet add package OpenTelemetry.Instrumentation.AspNetCore --version 1.7.0
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol --version 1.7.0
dotnet add package OpenTelemetry.Instrumentation.Http --version 1.7.0
dotnet add package OpenTelemetry.Exporter.Console --version 1.7.0

# Add Swagger for API documentation
dotnet add package Swashbuckle.AspNetCore --version 6.5.0
```

3. **Restore Dependencies:**
```bash
dotnet restore
```

4. **Configure OpenTelemetry:**

Update `Program.cs` with your OpenObserve credentials. You can find these in OpenObserve UI under Data Sources → Custom → Traces → OpenTelemetry → OTLP HTTP.

```csharp
builder.Services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .SetResourceBuilder(resourceBuilder)
        .AddSource(TracingInstrumentation.ServiceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri("your_openobserve_url/v1/traces");
            opts.Headers = "Authorization=Basic YOUR_AUTH_TOKEN";
            opts.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        }));
```

> Note: Remember to add `/v1/traces` to your OTLP HTTP endpoint

5. **Run the Application:**
```bash
dotnet run
```

## Using the API

1. **Access Swagger UI:**
   - Open `https://localhost:5xxx/swagger` in your browser
   - The exact port will be shown in the console when you run the application

2. **Create an Order:**
   - Using Swagger UI: Try the POST /order endpoint
   - Using curl:
```bash
curl -X POST https://localhost:5xxx/order \
  -H "Content-Type: application/json" \
  -d '{"customerName":"John Doe","amount":99.99}' \
  -k
```

3. **Retrieve an Order:**
   - Using Swagger UI: Try the GET /order/{id} endpoint
   - Using curl:
```bash
curl -k https://localhost:5xxx/order/1
```

## Viewing Traces in OpenObserve

1. Navigate to the Traces section
2. You should see traces appearing after making API requests

![Image](https://github.com/user-attachments/assets/062e5767-2774-4e93-bed1-d043d447a7a6)

> Note: There might be a slight delay before traces appear due to batching and export intervals

## Troubleshooting

If traces aren't appearing in OpenObserve:
- Verify your OpenObserve endpoint URL and authentication token
- Check if you've added `/v1/traces` to the endpoint URL
- Make some API requests to generate traces
- Wait a few moments for traces to be exported
- Check the application console for any export errors
