using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;

public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Configures Serilog to use OpenTelemetry as a sink.
    /// </summary>
    public static void ConfigureOpenTelemetry(LoggerConfiguration cfg, IConfiguration configuration)
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);
        if (useOtlpExporter)
        {
            cfg.WriteTo.OpenTelemetry(o =>
            {
                o.Endpoint = configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
                o.Protocol = Serilog.Sinks.OpenTelemetry.OtlpProtocol.Grpc;
            });
        }
    }

    /// <summary>
    /// Configures OpenTelemetry for logging, metrics, and tracing.
    /// </summary>
    public static TBuilder ConfigureOpenTelemetry<TBuilder>(TBuilder builder) where TBuilder : IHostApplicationBuilder
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
                    .AddRuntimeInstrumentation()
                    ;
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
}