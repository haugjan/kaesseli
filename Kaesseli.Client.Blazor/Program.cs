using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
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

if (builder.HostEnvironment.IsProduction())
{
    var apiScope = builder.Configuration["ApiScope"]
        ?? throw new InvalidOperationException("ApiScope not configured in appsettings.json");

    builder.Services.AddHttpClient("Kaesseli.API",
            client => client.BaseAddress = new Uri(apiBaseUrl))
        .AddHttpMessageHandler(sp => sp.GetRequiredService<AuthorizationMessageHandler>()
            .ConfigureHandler(
                authorizedUrls: [apiBaseUrl],
                scopes: [apiScope]));

    builder.Services.AddScoped(sp =>
        sp.GetRequiredService<IHttpClientFactory>().CreateClient("Kaesseli.API"));

    builder.Services.AddMsalAuthentication(options =>
    {
        builder.Configuration.Bind("AzureAdB2C", options.ProviderOptions.Authentication);
        options.ProviderOptions.DefaultAccessTokenScopes.Add(apiScope);
        options.ProviderOptions.LoginMode = "redirect";
    });
}
else
{
    builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });
}

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
