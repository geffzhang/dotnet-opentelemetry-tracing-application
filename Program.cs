using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using TracingDemo.Telemetry;
using Microsoft.OpenApi.Models;

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
            otlpOptions.Headers = "Authorization=Basic cm9vdEBleGFtcGxlLmNvbTpFNXcya2ZNRDVCQ082bzM5";
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

// Configure OpenTelemetry Tracing
builder.Services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .SetResourceBuilder(resourceBuilder)
        .AddSource(TracingInstrumentation.ServiceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri("http://localhost:5080/api/default/v1/traces");
            opts.Headers = "Authorization=Basic cm9vdEBleGFtcGxlLmNvbTpFNXcya2ZNRDVCQ082bzM5";
            opts.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
        }));

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