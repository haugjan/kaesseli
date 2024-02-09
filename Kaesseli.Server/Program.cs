using Kaesseli.Server.Accounts;
using Kaesseli.Server.Budget;
using Kaesseli.Server.Journal;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddBudgetRepositories(builder.Configuration);
builder.Services.AddApplicationServices();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.Services.InitializeDatabase();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapBudgetEndpoints();
app.MapJournalEndpoints();
app.MapAccountEndpoints();

app.MapFallbackToFile(filePath: "/index.html");

app.Run();