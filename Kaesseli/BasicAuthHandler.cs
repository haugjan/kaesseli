using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Kaesseli;

public class BasicAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IConfiguration configuration)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!AuthenticationHeaderValue.TryParse(Request.Headers.Authorization, out var header)
            || !"Basic".Equals(header.Scheme, StringComparison.OrdinalIgnoreCase)
            || string.IsNullOrEmpty(header.Parameter))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(header.Parameter));
        var separator = decoded.IndexOf(':');
        if (separator < 0)
            return Task.FromResult(AuthenticateResult.Fail("Invalid Basic auth header"));

        var username = decoded[..separator];
        var password = decoded[(separator + 1)..];

        var expectedUser = configuration["BasicAuth:Username"];
        var expectedPass = configuration["BasicAuth:Password"];

        if (username != expectedUser || password != expectedPass)
            return Task.FromResult(AuthenticateResult.Fail("Invalid credentials"));

        var claims = new[] { new Claim(ClaimTypes.Name, username) };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.WWWAuthenticate = "Basic realm=\"Kaesseli\"";
        Response.StatusCode = 401;
        return Task.CompletedTask;
    }
}
