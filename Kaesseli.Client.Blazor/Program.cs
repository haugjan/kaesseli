using System.Globalization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Http;
using Microsoft.JSInterop;
using Kaesseli.Client.Blazor;
using Kaesseli.Client.Blazor.Services;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("ApiBaseUrl not configured in appsettings.json");

builder.Services.AddScoped(_ =>
{
    var handler = new IncludeCredentialsHandler { InnerHandler = new HttpClientHandler() };
    return new HttpClient(handler) { BaseAddress = new Uri(apiBaseUrl) };
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

class IncludeCredentialsHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        return base.SendAsync(request, cancellationToken);
    }
}
