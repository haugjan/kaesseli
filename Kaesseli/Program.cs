using System.Diagnostics;
using Azure.Identity;
using Microsoft.AspNetCore.Authentication;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.user.json", optional: true, reloadOnChange: true);

var keyVaultUri = builder.Configuration["KeyVault:VaultUri"];
if (!string.IsNullOrEmpty(keyVaultUri))
{
    builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUri), new DefaultAzureCredential());
}

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// OpenTelemetry — only enabled when an OTLP endpoint is explicitly configured.
// Avoids gRPC connect attempts to a non-existent localhost collector in production.
var otlpEndpoint = builder.Configuration["Otlp:Endpoint"];
if (!string.IsNullOrEmpty(otlpEndpoint))
{
    AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
    Console.WriteLine($"[OTEL] Exporting to {otlpEndpoint}");

    var otlpApiKey = builder.Configuration["Otlp:ApiKey"] ?? "kaesseli-dev";

    void ConfigureOtlp(OpenTelemetry.Exporter.OtlpExporterOptions o)
    {
        o.Endpoint = new Uri(otlpEndpoint);
        o.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        o.Headers = $"x-otlp-api-key={otlpApiKey}";
    }

    builder
        .Services.AddOpenTelemetry()
        .ConfigureResource(resource => resource.AddService("Kaesseli"))
        .WithTracing(tracing =>
            tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("Microsoft.EntityFrameworkCore")
                .AddOtlpExporter(ConfigureOtlp)
        )
        .WithMetrics(metrics =>
            metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter(ConfigureOtlp)
        );

    builder.Logging.AddOpenTelemetry(logging =>
    {
        logging.IncludeScopes = true;
        logging.IncludeFormattedMessage = true;
        logging.AddOtlpExporter(ConfigureOtlp);
    });
}

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowSpecificOrigin",
        b =>
            b.WithOrigins(
                    "http://localhost:9500",
                    "https://localhost:9500",
                    "http://localhost:9501",
                    "https://localhost:9501",
                    "http://localhost:9000",
                    "http://localhost:7033",
                    "https://localhost:7033"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
    );
});

builder
    .Services.AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, Kaesseli.BasicAuthHandler>("Basic", null);

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");

app.UseAuthentication();
app.Use(
    async (context, next) =>
    {
        // Allow health endpoint without auth so external keep-alive pings can reach it.
        if (context.Request.Path.StartsWithSegments("/healthz"))
        {
            await next();
            return;
        }
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await context.ChallengeAsync("Basic");
            return;
        }
        await next();
    }
);

app.MapHealthChecks("/healthz");

app.UseBlazorFrameworkFiles();
app.UseDefaultFiles();
app.UseStaticFiles();

// Run database initialization in background so it does not block app startup.
// Cosmos EnsureCreatedAsync makes several round-trips and adds seconds to cold start.
_ = Task.Run(async () =>
{
    try
    {
        using var scope = app.Services.CreateScope();
        var context =
            scope.ServiceProvider.GetRequiredService<Kaesseli.Infrastructure.KaesseliContext>();
        await context.Database.EnsureCreatedAsync();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[STARTUP] Database initialization failed: {ex.Message}");
    }
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    _ = Task.Run(async () =>
    {
        await Task.Delay(1000);
        try
        {
            using var client = new HttpClient();
            await client.GetAsync("http://localhost:18888");
            Process.Start(
                new ProcessStartInfo("http://localhost:18888") { UseShellExecute = true }
            );
        }
        catch
        {
            // Aspire Dashboard not running, skip
        }
    });
}

app.MapKaesseliEndpoints();

app.MapFallbackToFile("/index.html");

app.Run();
