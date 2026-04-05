using Kaesseli.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

var configuration = new ConfigurationBuilder()
    .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"), optional: true)
    .AddJsonFile(Path.Combine(AppContext.BaseDirectory, $"appsettings.{environment}.json"), optional: true)
    .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.user.json"), optional: true)
    .Build();

var services = new ServiceCollection();
services.AddLogging();
services.AddSingleton<IConfiguration>(configuration);
services.AddSingleton(TimeProvider.System);
services.AddInfrastructureServices(configuration);

await using var provider = services.BuildServiceProvider();

Console.WriteLine("Initializing CosmosDB database...");
await provider.InitializeDatabaseAsync();
Console.WriteLine("Database initialized successfully.");

if (environment == "Development")
{
    Console.WriteLine("Seeding development data...");
    await provider.SeedDevelopmentDataAsync();
    Console.WriteLine("Development data seeded successfully.");
}
