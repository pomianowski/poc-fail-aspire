using Aspire.AppHost.Extensions;
using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using Projects;

const int variant = 4;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

builder.AddForwardedHeaders();

IResourceBuilder<IResourceWithConnectionString> postgres;

// #1 Variant 1, does work
if (variant == 1)
{
    postgres = builder.AddPostgres("my-postgres").WithImage("postgres").WithImageTag("alpine");
}

// #2 Variant 2, does NOT work
if (variant == 2)
{
    postgres = builder
        .AddPostgres("my-postgres")
        .WithDataVolume() // causes exception Npgsql.PostgresException (0x80004005): 28P01: password authentication failed for user "postgres"
        .WithImage("postgres")
        .WithImageTag("alpine");
}

// #3 Variant 3, does work
if (variant == 3)
{
    IResourceBuilder<PostgresServerResource> postgresBuilder = builder
        .AddPostgres("my-postgres")
        .WithImage("postgres")
        .WithImageTag("alpine");

    postgres = postgresBuilder.AddDatabase("my-postgres-db");
}

// #4 Variant 4, does NOT work
if (variant == 4)
{
    IResourceBuilder<PostgresServerResource> postgresBuilder = builder
        .AddPostgres("my-postgres")
        .WithDataVolume() // causes exception Npgsql.PostgresException (0x80004005): 28P01: password authentication failed for user "postgres"
        .WithImage("postgres")
        .WithImageTag("alpine");

    postgres = postgresBuilder.AddDatabase("my-postgres-db");
}

_ = builder
    .AddProject<AspirePoc_App>("my-app")
    .WithOtlpExporter()
    .WithReference(postgres, "TestConnection")
    .WithEnvironment("StartupDelayInSeconds", "60");

await using DistributedApplication app = builder.Build();

await app.RunAsync();
