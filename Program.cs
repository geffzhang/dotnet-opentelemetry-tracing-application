using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TracingDemo.Telemetry;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Instrumentation.Http;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Tracing Demo API", Version = "v1" });
});
builder.Services.AddHttpClient();

// Configure OpenTelemetry
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(TracingInstrumentation.ServiceName)
    .AddAttributes(new Dictionary<string, object>
    {
        ["environment"] = "development",
        ["service.version"] = "1.0.0"
    });

builder.Services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .SetResourceBuilder(resourceBuilder)
        .AddSource(TracingInstrumentation.ServiceName)
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddConsoleExporter()
        .AddOtlpExporter(opts =>
        {
            opts.Endpoint = new Uri("http://localhost:5080/api/default/v1/traces");
            opts.Headers = "Authorization= Basic cm9vdEBleGFtcGxlLmNvbTpFNXcya2ZNRDVCQ082bzM5";
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