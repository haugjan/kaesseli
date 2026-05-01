using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;

namespace Kaesseli;

public static class GoogleAuth
{
    public const string PolicyName = "GoogleAllowlist";
    public const string AllowedEmailsConfigKey = "Auth:Google:AllowedEmails";
    public const string MetadataPath = "/auth/google/.well-known/openid-configuration";
    public const string TokenProxyPath = "/auth/google/token";
    private const string GoogleIssuer = "https://accounts.google.com";
    private const string GoogleMetadataUrl =
        "https://accounts.google.com/.well-known/openid-configuration";
    private const string GoogleTokenUrl = "https://oauth2.googleapis.com/token";

    public static IServiceCollection AddGoogleAuth(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var clientId =
            configuration["Auth:Google:ClientId"]
            ?? throw new InvalidOperationException(
                "Auth:Google:ClientId is not configured. Set it in appsettings.user.json or KeyVault."
            );

        services.AddHttpClient();

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = GoogleIssuer;
                options.Audience = clientId;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuers = [GoogleIssuer, "accounts.google.com"],
                    ValidAudience = clientId,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    NameClaimType = "email",
                };
            });

        services
            .AddAuthorizationBuilder()
            .AddPolicy(
                PolicyName,
                policy =>
                    policy
                        .RequireAuthenticatedUser()
                        .AddRequirements(new EmailAllowlistRequirement())
            )
            .SetFallbackPolicy(
                new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .AddRequirements(new EmailAllowlistRequirement())
                    .Build()
            );

        services.AddSingleton<IAuthorizationHandler, EmailAllowlistHandler>();

        return services;
    }

    public static IEndpointRouteBuilder MapGoogleAuthProxy(this IEndpointRouteBuilder app)
    {
        app.MapGet(
                MetadataPath,
                async (HttpContext ctx, IHttpClientFactory factory) =>
                {
                    var http = factory.CreateClient();
                    var metadata =
                        await http.GetFromJsonAsync<JsonObject>(GoogleMetadataUrl)
                        ?? throw new InvalidOperationException(
                            "Could not fetch Google OIDC metadata."
                        );

                    var scheme =
                        ctx.Request.Headers.TryGetValue("X-Forwarded-Proto", out var xfp)
                        && xfp.Count > 0
                            ? xfp[0]!
                            : ctx.Request.Scheme;
                    var origin = $"{scheme}://{ctx.Request.Host}";
                    metadata["token_endpoint"] = $"{origin}{TokenProxyPath}";

                    return Results.Content(metadata.ToJsonString(), "application/json");
                }
            )
            .AllowAnonymous();

        app.MapPost(
                TokenProxyPath,
                async (
                    HttpContext ctx,
                    IConfiguration configuration,
                    IHttpClientFactory factory,
                    ILogger<EmailAllowlistHandler> logger
                ) =>
                {
                    var clientSecret = configuration["Auth:Google:ClientSecret"];
                    if (string.IsNullOrEmpty(clientSecret))
                        return Results.Problem(
                            "Auth:Google:ClientSecret is not configured.",
                            statusCode: 500
                        );

                    using var reader = new StreamReader(ctx.Request.Body);
                    var rawBody = await reader.ReadToEndAsync();

                    var form = QueryHelpers.ParseQuery(rawBody);
                    var pairs = form.Where(kv =>
                            !string.Equals(kv.Key, "client_secret", StringComparison.Ordinal)
                        )
                        .SelectMany(kv =>
                            kv.Value.Select(v => new KeyValuePair<string, string>(
                                kv.Key,
                                v ?? string.Empty
                            ))
                        )
                        .Append(new KeyValuePair<string, string>("client_secret", clientSecret));

                    var http = factory.CreateClient();
                    using var response = await http.PostAsync(
                        GoogleTokenUrl,
                        new FormUrlEncodedContent(pairs)
                    );
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var contentType =
                        response.Content.Headers.ContentType?.ToString() ?? "application/json";
                    return Results.Content(
                        responseBody,
                        contentType,
                        statusCode: (int)response.StatusCode
                    );
                }
            )
            .AllowAnonymous()
            .DisableAntiforgery();

        return app;
    }
}

public sealed class EmailAllowlistRequirement : IAuthorizationRequirement;

public sealed class EmailAllowlistHandler(
    IConfiguration configuration,
    ILogger<EmailAllowlistHandler> logger
) : AuthorizationHandler<EmailAllowlistRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        EmailAllowlistRequirement requirement
    )
    {
        var allowed =
            configuration.GetSection(GoogleAuth.AllowedEmailsConfigKey).Get<string[]>() ?? [];

        if (allowed.Length == 0)
        {
            logger.LogWarning("Auth:Google:AllowedEmails is empty — no user will be authorized.");
            return Task.CompletedTask;
        }

        var emailVerified = context.User.FindFirstValue("email_verified");
        if (!string.Equals(emailVerified, "true", StringComparison.OrdinalIgnoreCase))
            return Task.CompletedTask;

        var email =
            context.User.FindFirstValue("email") ?? context.User.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrEmpty(email))
            return Task.CompletedTask;

        if (allowed.Any(a => string.Equals(a, email, StringComparison.OrdinalIgnoreCase)))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
