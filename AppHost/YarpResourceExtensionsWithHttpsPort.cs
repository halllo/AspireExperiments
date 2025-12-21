using System.Reflection;
using Aspire.Hosting.Yarp;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Until https://github.com/dotnet/aspire/issues/13674 is resolved, we inline the Yarp registration logic to provide the HTTPS port for YARP resources.
/// </summary>
public static class YarpResourceExtensionsWithHttpsPort
{
   public static IResourceBuilder<YarpResource> AddYarpWithHttpsHostPort(this IDistributedApplicationBuilder builder, [ResourceName] string name, int httpsHostPort)
   {
#pragma warning disable ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
      var resource = new YarpResource(name);
      const int port = 5000;
      const int httpsPort = 5001;

      // Use reflection to get values from internal YarpContainerImageTags2
      var yarpTagsType = Type.GetType("Aspire.Hosting.Yarp.YarpContainerImageTags, Aspire.Hosting.Yarp");
      if (yarpTagsType == null) throw new InvalidOperationException("Could not find YarpContainerImageTags2 type");
      string YarpContainerImageTags(string name) => (string)yarpTagsType.GetField(name, BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;

      var yarpBuilder = builder.AddResource(resource)
         .WithHttpEndpoint(name: "http", targetPort: port)
         .WithImage(YarpContainerImageTags("Image"))
         .WithImageRegistry(YarpContainerImageTags("Registry"))
         .WithImageTag(YarpContainerImageTags("Tag"))
         .WithEnvironment("ASPNETCORE_ENVIRONMENT", builder.Environment.EnvironmentName)
         .WithEntrypoint("dotnet")
         .WithArgs("/app/yarp.dll")
         .WithOtlpExporter()
         .WithHttpsCertificateConfiguration(ctx =>
         {
            ctx.EnvironmentVariables["Kestrel__Certificates__Default__Path"] = ctx.CertificatePath;
            ctx.EnvironmentVariables["Kestrel__Certificates__Default__KeyPath"] = ctx.KeyPath;
            if (ctx.Password is not null)
            {
               ctx.EnvironmentVariables["Kestrel__Certificates__Default__Password"] = ctx.Password;
            }

            return Task.CompletedTask;
         });

      if (builder.ExecutionContext.IsRunMode)
      {
         builder.Eventing.Subscribe<BeforeStartEvent>((@event, cancellationToken) =>
         {
            var developerCertificateService = @event.Services.GetRequiredService<IDeveloperCertificateService>();

            bool addHttps = false;
            if (!resource.TryGetLastAnnotation<HttpsCertificateAnnotation>(out var annotation))
            {
               if (developerCertificateService.UseForHttps)
               {
                  // If no specific certificate is configured
                  addHttps = true;
               }
            }
            else if (annotation.UseDeveloperCertificate.GetValueOrDefault(developerCertificateService.UseForHttps) || annotation.Certificate is not null)
            {
               addHttps = true;
            }

            if (addHttps)
            {
               // If a TLS certificate is configured, ensure the YARP resource has an HTTPS endpoint and
               // configure the environment variables to use it.
               yarpBuilder
                  //.WithHttpsEndpoint(targetPort: httpsPort)
                  .WithEndpoint("https", ep =>
                     {
                        // Create or update the HTTPS endpoint
                        ep.TargetPort ??= httpsPort;
                        ep.UriScheme = "https";
                        ep.Port ??= httpsHostPort;
                     }, createIfNotExists: true)
                     .WithEnvironment("ASPNETCORE_HTTPS_PORT", resource.GetEndpoint("https").Property(EndpointProperty.Port))
                     .WithEnvironment("ASPNETCORE_URLS", $"{resource.GetEndpoint("https").Property(EndpointProperty.Scheme)}://*:{resource.GetEndpoint("https").Property(EndpointProperty.TargetPort)};{resource.GetEndpoint("http").Property(EndpointProperty.Scheme)}://*:{resource.GetEndpoint("http").Property(EndpointProperty.TargetPort)}");
            }

            return Task.CompletedTask;
         });
      }

      if (builder.ExecutionContext.IsRunMode)
      {
         yarpBuilder.WithEnvironment(ctx =>
         {
            var developerCertificateService = ctx.ExecutionContext.ServiceProvider.GetRequiredService<IDeveloperCertificateService>();
            if (!developerCertificateService.SupportsContainerTrust)
            {
               // On systems without the ASP.NET DevCert updates introduced in .NET 10, YARP will not trust the cert used
               // by Aspire otlp endpoint when running locally. The Aspire otlp endpoint uses the dev cert, and prior to
               // .NET 10, it was only valid for localhost, but from the container perspective, the url will be something
               // like https://docker.host.internal, so it will NOT be valid. This is not necessary when using the latest
               // dev cert.
               ctx.EnvironmentVariables["YARP_UNSAFE_OLTP_CERT_ACCEPT_ANY_SERVER_CERTIFICATE"] = "true";
            }
         });
      }

      yarpBuilder.WithEnvironment(ctx =>
      {
         //YarpEnvConfigGenerator.PopulateEnvVariables(ctx.EnvironmentVariables, yarpBuilder.Resource.Routes, yarpBuilder.Resource.Clusters);
         
         var routes = yarpBuilder.Resource.GetType().GetProperty("Routes", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(yarpBuilder.Resource);
         var clusters = yarpBuilder.Resource.GetType().GetProperty("Clusters", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(yarpBuilder.Resource);
         
         var yarpEnvConfigGeneratorType = Type.GetType("Aspire.Hosting.YarpEnvConfigGenerator, Aspire.Hosting.Yarp");
         if (yarpEnvConfigGeneratorType == null) throw new InvalidOperationException("Could not find YarpEnvConfigGenerator type");
         var populateEnvVariables = yarpEnvConfigGeneratorType.GetMethod("PopulateEnvVariables", BindingFlags.Static | BindingFlags.Public);
         if (populateEnvVariables == null) throw new InvalidOperationException("Could not find PopulateEnvVariables method");
         populateEnvVariables.Invoke(null, [ctx.EnvironmentVariables, routes, clusters]);
      });

      return yarpBuilder;
#pragma warning restore ASPIRECERTIFICATES001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
   }
}
