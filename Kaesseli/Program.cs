using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.user.json", optional: true, reloadOnChange: true);

builder.Services.AddOpenApi();

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
                    "https://localhost:7033")
                .AllowAnyHeader()
                .AllowAnyMethod()
    );
});

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowSpecificOrigin");

app.UseDefaultFiles();
app.UseStaticFiles();
await app.Services.InitializeDatabaseAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapKaesseliEndpoints();

app.MapFallbackToFile("/index.html");

app.Run();
