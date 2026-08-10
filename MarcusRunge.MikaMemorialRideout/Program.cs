using MarcusRunge.MikaMemorialRideout;
using MarcusRunge.MikaMemorialRideout.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseAddress = builder.Configuration["ApiBaseAddress"];
var baseAddress = string.IsNullOrWhiteSpace(apiBaseAddress)
    ? builder.HostEnvironment.BaseAddress
    : apiBaseAddress;

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(baseAddress) });
builder.Services.AddScoped<IRideoutApiClient, RideoutApiClient>();

await builder.Build().RunAsync();
