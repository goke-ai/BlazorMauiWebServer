using Goke.Core.Interfaces;
using GokeWebServer.Client.Services;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();

// Add other services
builder.Services.AddTransient<IFormFactor, ClientFormFactorService>();


await builder.Build().RunAsync();
