using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

ConfigureOpenTelemetry(builder);

builder.Services.AddHealthChecks()
	.AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
	// All health checks must pass for app to be considered ready to accept traffic after starting
	app.MapHealthChecks("/health");

	// Only health checks tagged with the "live" tag must pass for app to be considered alive
	app.MapHealthChecks("/alive", new HealthCheckOptions
	{
		Predicate = r => r.Tags.Contains("live")
	});
}

app.Run();


static TBuilder ConfigureOpenTelemetry<TBuilder>(TBuilder builder) where TBuilder : IHostApplicationBuilder
{
	builder.Logging.AddOpenTelemetry(logging =>
	{
		logging.IncludeFormattedMessage = true;
		logging.IncludeScopes = true;
	});

	builder.Services.AddOpenTelemetry()
		.WithMetrics(metrics =>
		{
			metrics.AddAspNetCoreInstrumentation()
				.AddHttpClientInstrumentation()
				.AddRuntimeInstrumentation();
		})
		.WithTracing(tracing =>
		{
			const string HealthEndpointPath = "/health";
			const string AlivenessEndpointPath = "/alive";
			tracing.AddSource(builder.Environment.ApplicationName)
				.AddAspNetCoreInstrumentation(tracing =>
					// Exclude health check requests from tracing
					tracing.Filter = context =>
						!context.Request.Path.StartsWithSegments(HealthEndpointPath)
						&& !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
				)
				// Uncomment the following line to enable gRPC instrumentation (requires the OpenTelemetry.Instrumentation.GrpcNetClient package)
				//.AddGrpcClientInstrumentation()
				.AddHttpClientInstrumentation();
		});

	var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
	if (useOtlpExporter)
	{
		builder.Services.AddOpenTelemetry().UseOtlpExporter();
	}

	return builder;
}
