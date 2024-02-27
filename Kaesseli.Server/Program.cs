var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(
    options =>
    {
        options.AddPolicy(
            name: "AllowSpecificOrigin",
            b => b.WithOrigins("http://localhost:9001")
                  .WithOrigins("https://localhost:9001")
                  .AllowAnyHeader()
                  .AllowAnyMethod());
    });

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.Services.InitializeDatabase();
app.UseCors(policyName: "AllowSpecificOrigin");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapKaesseliEndpoints();

app.MapFallbackToFile(filePath: "/index.html");

app.Run();