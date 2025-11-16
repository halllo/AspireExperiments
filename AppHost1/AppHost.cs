var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.WebApplication1>("Web1");

builder.Build().Run();
