var builder = DistributedApplication.CreateBuilder(args);

var identity = builder.AddProject<Projects.Identity>("Identity");

var backend = builder.AddProject<Projects.Backend>("Backend");

var frontend = builder.AddViteApp("Frontend", "../Frontend");

var gateway = builder
                     //.AddYarp("Gateway")
                     //.WithHttpsEndpoint(8443)
                     .AddYarpWithHttpsHostPort("Gateway", 8443)
                     .WithHostPort(8080)
                     .WithConfiguration(yarp =>
                     {
                        yarp.AddRoute("/identity/{**catch-all}", identity);
                        yarp.AddRoute("/backend/{**catch-all}", backend);
                        yarp.AddRoute("/frontend/{**catch-all}", frontend);
                     });

builder.Build().Run();
