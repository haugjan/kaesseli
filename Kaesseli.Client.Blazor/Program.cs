using System.Globalization;
using Kaesseli.Client.Blazor;
using Kaesseli.Client.Blazor.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using MudBlazor.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var baseAddress = builder.HostEnvironment.BaseAddress.TrimEnd('/');
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Auth:Google", options.ProviderOptions);
    options.ProviderOptions.Authority = "https://accounts.google.com";
    options.ProviderOptions.MetadataUrl =
        $"{baseAddress}/auth/google/.well-known/openid-configuration";
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.RedirectUri = $"{baseAddress}/authentication/login-callback";
    options.ProviderOptions.PostLogoutRedirectUri = $"{baseAddress}/authentication/logout-callback";
    options.ProviderOptions.DefaultScopes.Clear();
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("email");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.UserOptions.NameClaim = "email";
});

builder.Services.AddScoped<IdTokenAuthorizationMessageHandler>();
builder.Services.AddScoped(sp =>
{
    var handler = sp.GetRequiredService<IdTokenAuthorizationMessageHandler>();
    handler.InnerHandler = new HttpClientHandler();
    return new HttpClient(handler) { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
});

builder.Services.AddScoped<KaesseliApiService>();
builder.Services.AddSingleton<AccountingPeriodState>();
builder.Services.AddSingleton<PwaUpdateService>();
builder.Services.AddMudServices();

var host = builder.Build();

var js = host.Services.GetRequiredService<IJSRuntime>();
var browserLang = await js.InvokeAsync<string>("eval", "navigator.language");
var culture = new CultureInfo(browserLang);
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

await host.RunAsync();
