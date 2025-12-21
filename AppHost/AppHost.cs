var builder = DistributedApplication.CreateBuilder(args);

var identity = builder.AddProject<Projects.Identity>("Identity");

var backend = builder.AddProject<Projects.Backend>("Backend");

var frontend = builder.AddViteApp("Frontend", "../Frontend");

var gateway = builder.AddYarp("Gateway")
                     //.WithHostHttpsPort(8443) //https://github.com/dotnet/aspire/issues/13674
                     .WithHostPort(8080)
                     .WithConfiguration(yarp =>
                     {
                        yarp.AddRoute("/identity/{**catch-all}", identity);
                        yarp.AddRoute("/backend/{**catch-all}", backend);
                        yarp.AddRoute("/frontend/{**catch-all}", frontend);
                     });

builder.Eventing.Subscribe<BeforeStartEvent>((_, _) =>
{
   gateway.WithHostHttpsPort(8443);
   return Task.CompletedTask;
});

builder.Build().Run();
