using MarcusRunge.MikaMemorialRideout.Api.Security;
using MarcusRunge.MikaMemorialRideout.Api.Storage;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IRegistrationRepository, TableRegistrationRepository>();
builder.Services.AddSingleton<IRegistrationStateRepository, TableRegistrationStateRepository>();
builder.Services.AddSingleton<IPlanningStatusRepository, TablePlanningStatusRepository>();
builder.Services.AddSingleton<IAdminCodeVerifier, AdminCodeVerifier>();

builder.Build().Run();
