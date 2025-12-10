var builder = DistributedApplication.CreateBuilder(args);

var backend = builder.AddProject<Projects.Backend>("Backend");

var frontend = builder.AddViteApp("Frontend", "../Frontend");

var gateway = builder.AddYarp("Gateway")
                     .WithConfiguration(yarp =>
                     {
                        yarp.AddRoute("/frontend/{**catch-all}", frontend);
                        yarp.AddRoute("/backend/{**catch-all}", backend);
                     });

builder.Build().Run();
