var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.Backend>("Backend");

builder.AddNpmApp("Frontend", "../Frontend").WithUrl("http://localhost:4200");

builder.Build().Run();
