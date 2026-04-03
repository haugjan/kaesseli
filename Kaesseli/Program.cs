using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.user.json", optional: true, reloadOnChange: true);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "AllowSpecificOrigin",
        b =>
            b.WithOrigins("http://localhost:9500")
                .WithOrigins("https://localhost:9500")
                .WithOrigins("http://localhost:9501")
                .WithOrigins("https://localhost:9501")
                .WithOrigins("http://localhost:9000")
                .AllowAnyHeader()
                .AllowAnyMethod()
    );
});

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
await app.Services.InitializeDatabaseAsync();
app.UseCors("AllowSpecificOrigin");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseAuthentication();
// app.UseAuthorization();

app.UseHttpsRedirection();

app.MapKaesseliEndpoints();

app.MapFallbackToFile("/index.html");

app.Run();
