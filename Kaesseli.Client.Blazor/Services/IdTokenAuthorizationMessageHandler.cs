using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;

namespace Kaesseli.Client.Blazor.Services;

public sealed class IdTokenAuthorizationMessageHandler(IJSRuntime js, NavigationManager navigation)
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var idToken = await js.InvokeAsync<string?>(
                "kaesseliAuth.getIdToken",
                cancellationToken
            );

            if (!string.IsNullOrEmpty(idToken))
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", idToken);
        }
        catch (JSException) { }

        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized && !IsOnAuthenticationPage())
            navigation.NavigateToLogin("authentication/login");

        return response;
    }

    private bool IsOnAuthenticationPage()
    {
        var path = navigation.ToBaseRelativePath(navigation.Uri);
        return path.StartsWith("authentication/", StringComparison.OrdinalIgnoreCase);
    }
}
