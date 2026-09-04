var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.GokeWebServer>("gokewebserver");

builder.Build().Run();
