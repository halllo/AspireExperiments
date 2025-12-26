var builder = DistributedApplication.CreateBuilder(args);

var identity = builder.AddProject<Projects.Identity>("Identity");

var backend = builder.AddProject<Projects.Backend>("Backend");

var element = builder.AddViteApp("Element", "../Element");

var frontend = builder.AddViteApp("Frontend", "../Frontend");

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
