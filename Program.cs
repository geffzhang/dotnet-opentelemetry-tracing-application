using Microsoft.OpenApi.Models;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Text;
using TracingDemo.Telemetry;

// These are extension methods, we don't need their namespaces
// using OpenTelemetry.Instrumentation.AspNetCore;
// using OpenTelemetry.Instrumentation.Http;

var builder = WebApplication.CreateBuilder(args);

// Clear existing providers and configure OpenTelemetry Logging
builder.Logging.ClearProviders();

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(TracingInstrumentation.ServiceName)
    .AddAttributes(new Dictionary<string, object>
    {
        ["environment"] = "development",
        ["service.version"] = "1.0.0"
    });

builder.Logging.AddOpenTelemetry(logging => {
    logging.IncludeFormattedMessage = true;
    logging.SetResourceBuilder(resourceBuilder)
        .AddConsoleExporter()  // Keep console logging for debugging
        .AddOtlpExporter(otlpOptions => {
            otlpOptions.Endpoint = new Uri("http://localhost:5080/api/default/v1/logs");
            otlpOptions.Headers = "Authorization=Basic cm9vdEBleGFtcGxlLmNvbTpYdmdNN0c2MklSODd0V1J0";
            otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        });
});

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tracing Demo API", Version = "v1" });
});
builder.Services.AddHttpClient();

builder.Services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .SetResourceBuilder(resourceBuilder)
        .AddSource(TracingInstrumentation.ServiceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri("http://localhost:5080/api/default/v1/traces");
            opts.Headers = "Authorization=Basic cm9vdEBleGFtcGxlLmNvbTpYdmdNN0c2MklSODd0V1J0";
            opts.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        })
   )
    .WithMetrics(metricsBuilder => metricsBuilder
    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(TracingInstrumentation.ServiceName))
    .AddRuntimeInstrumentation() // This line will now work
    .AddHttpClientInstrumentation()
    .AddAspNetCoreInstrumentation()
    .AddConsoleExporter()
    .AddOtlpExporter(configure =>
    {
        configure.BatchExportProcessorOptions = new BatchExportProcessorOptions<Activity>
        {
            MaxQueueSize = 100,        // 减小队列避免堆积
            MaxExportBatchSize = 10,   // 减小批量大小
            ScheduledDelayMilliseconds = 500, // 更频繁导出
        };
        configure.Endpoint = new Uri($"http://localhost:5080/api/default/v1/metrics"); // OpenObserve endpoint
        configure.Headers = $"Authorization=Basic cm9vdEBleGFtcGxlLmNvbTpYdmdNN0c2MklSODd0V1J0";
        configure.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
    })
);


var app = builder.Build();
       
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tracing Demo API v1"));
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run(); 