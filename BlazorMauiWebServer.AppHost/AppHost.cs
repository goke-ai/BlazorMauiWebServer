var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.GokeWebServer>("gokewebserver");

builder.AddProject<Projects.GokeWeb>("gokeweb");

builder.AddProject<Projects.GokeHyb>("gokehyb");

builder.Build().Run();
