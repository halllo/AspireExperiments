var builder = DistributedApplication.CreateBuilder(args);

var identity = builder.AddProject<Projects.Identity>("Identity");

var backend = builder.AddProject<Projects.Backend>("Backend");

var element = builder.AddViteApp("Element", "../Element");

var frontend = builder.AddViteApp("Frontend", "../Frontend");

var appHostDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

var prometheus = builder.AddContainer("Prometheus", "prom/prometheus")
   .WithBindMount(Path.Combine(appHostDirectory, "observability", "prometheus", "prometheus.yml"), "/etc/prometheus/prometheus.yml", isReadOnly: true)
   .WithArgs("--config.file=/etc/prometheus/prometheus.yml")
   .WithHttpEndpoint(port: 9090, targetPort: 9090);

var grafana = builder.AddContainer("Grafana", "grafana/grafana")
   .WithEnvironment("GF_SECURITY_ADMIN_USER", "admin")
   .WithEnvironment("GF_SECURITY_ADMIN_PASSWORD", "admin")
   .WithBindMount(Path.Combine(appHostDirectory, "observability", "grafana", "provisioning"), "/etc/grafana/provisioning", isReadOnly: true)
   .WithBindMount(Path.Combine(appHostDirectory, "observability", "grafana", "dashboards"), "/var/lib/grafana/dashboards", isReadOnly: true)
   .WithHttpEndpoint(port: 3000, targetPort: 3000);

var gateway = builder.AddYarp("Gateway")
   //.WithHostHttpsPort(8443) //https://github.com/dotnet/aspire/issues/13674
   .WithHostPort(8080)
   .WithStaticFiles("../wwwroot")
   .WithConfiguration(yarp =>
   {
      yarp.AddRoute("/identity/{**catch-all}", identity);
      yarp.AddRoute("/backend/{**catch-all}", backend);
      yarp.AddRoute("/element/{**catch-all}", element);
      yarp.AddRoute("/frontend/{**catch-all}", frontend);
   });

var externalFrontend = builder.AddViteApp("ExternalFrontend", "../ExternalFrontend");

var externalGateway = builder.AddYarp("ExternalGateway")
   //.WithHostHttpsPort(9443) //https://github.com/dotnet/aspire/issues/13674
   .WithHostPort(9080)
   .WithConfiguration(yarp =>
   {
      yarp.AddRoute("{**catch-all}", externalFrontend);
   });

//https://github.com/dotnet/aspire/issues/13674
builder.Eventing.Subscribe<BeforeStartEvent>((_, _) =>
{
   gateway.WithHostHttpsPort(8443);
   externalGateway.WithHostHttpsPort(9443);
   return Task.CompletedTask;
});

builder.Build().Run();
