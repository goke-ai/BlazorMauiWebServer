using Goke.Core.Interfaces;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Add other services
builder.Services.AddTransient<IFormFactor, GokeWebServer.Client.Services.FormFactorService>();


await builder.Build().RunAsync();
