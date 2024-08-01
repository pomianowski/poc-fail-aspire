using AspirePoc.App;
using Microsoft.EntityFrameworkCore;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

string? connectionString = builder.Configuration.GetConnectionString("TestConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddHostedService<DatabaseTesterBackgroundService>();

WebApplication app = builder.Build();

app.MapGet("/healthz", () => "Alive!");

await app.RunAsync();
