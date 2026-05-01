using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Kaesseli.Client.Blazor;
using Kaesseli.Client.Blazor.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl not configured in appsettings.json");

var basicUser = builder.Configuration["BasicAuth:Username"];
var basicPass = builder.Configuration["BasicAuth:Password"];
    AuthenticationHeaderValue? basicAuthHeader = null;
    if (!string.IsNullOrEmpty(basicUser) && !string.IsNullOrEmpty(basicPass))
    {
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{basicUser}:{basicPass}"));
        basicAuthHeader = new AuthenticationHeaderValue("Basic", encoded);
    }

    builder.Services.AddScoped(_ =>
    {
        var client = new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
        if (basicAuthHeader is not null)
            client.DefaultRequestHeaders.Authorization = basicAuthHeader;
        return client;
    });

    builder.Services.AddScoped<KaesseliApiService>();
builder.Services.AddSingleton<AccountingPeriodState>();
builder.Services.AddMudServices();

var host = builder.Build();

var js = host.Services.GetRequiredService<IJSRuntime>();
var browserLang = await js.InvokeAsync<string>("eval", "navigator.language");
var culture = new CultureInfo(browserLang);
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await host.RunAsync();
