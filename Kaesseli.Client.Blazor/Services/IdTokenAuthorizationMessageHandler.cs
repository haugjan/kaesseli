using System.Net.Http.Headers;
using Microsoft.JSInterop;

namespace Kaesseli.Client.Blazor.Services;

public sealed class IdTokenAuthorizationMessageHandler(IJSRuntime js) : DelegatingHandler
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

        return await base.SendAsync(request, cancellationToken);
    }
}
