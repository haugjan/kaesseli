using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kaesseli.Features.AccountSuggestion.Gemini;

public record GeminiAccountSuggestion(string AccountShortName, double Confidence);

public record GeminiAccountInfo(string ShortName, string Name, string Type, string Number);

public record GeminiHistoricalAssignment(string Description, decimal Amount, string AccountShortName);

public interface IGeminiClient
{
    Task<IReadOnlyList<GeminiAccountSuggestion>> SuggestAccountsAsync(
        string description,
        decimal amount,
        IReadOnlyList<GeminiAccountInfo> accountPlan,
        IReadOnlyList<GeminiHistoricalAssignment> examples,
        CancellationToken cancellationToken
    );
}

public class GeminiOptions
{
    public string? ApiKey { get; set; }
    public string Model { get; set; } = "gemini-2.5-flash";
    public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com";
    public int MaxExamples { get; set; } = 50;
    public int TopN { get; set; } = 3;
}

public class GeminiClient(
    HttpClient httpClient,
    GeminiOptions options,
    ILogger<GeminiClient> logger
) : IGeminiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public async Task<IReadOnlyList<GeminiAccountSuggestion>> SuggestAccountsAsync(
        string description,
        decimal amount,
        IReadOnlyList<GeminiAccountInfo> accountPlan,
        IReadOnlyList<GeminiHistoricalAssignment> examples,
        CancellationToken cancellationToken
    )
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
            throw new InvalidOperationException("Gemini:ApiKey is not configured.");

        var prompt = BuildPrompt(description, amount, accountPlan, examples);
        var request = new GeminiRequest
        {
            Contents = [new GeminiContent { Parts = [new GeminiPart { Text = prompt }] }],
            GenerationConfig = new GeminiGenerationConfig
            {
                ResponseMimeType = "application/json",
                ResponseSchema = BuildResponseSchema(),
                Temperature = 0.1,
            },
        };

        var url = $"{options.Endpoint.TrimEnd('/')}/v1beta/models/{options.Model}:generateContent?key={options.ApiKey}";
        using var response = await httpClient.PostAsJsonAsync(url, request, JsonOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            logger.LogWarning(
                "Gemini call failed with {Status}: {Body}",
                response.StatusCode,
                body
            );
            response.EnsureSuccessStatusCode();
        }

        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(JsonOptions, cancellationToken);
        var json = result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(json))
            return [];

        try
        {
            var suggestions = JsonSerializer.Deserialize<List<GeminiAccountSuggestion>>(json, JsonOptions);
            return suggestions ?? [];
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Gemini returned non-JSON payload: {Json}", json);
            return [];
        }
    }

    private string BuildPrompt(
        string description,
        decimal amount,
        IReadOnlyList<GeminiAccountInfo> accountPlan,
        IReadOnlyList<GeminiHistoricalAssignment> examples
    )
    {
        var planLines = string.Join(
            Environment.NewLine,
            accountPlan.Select(a => $"- {a.ShortName} | {a.Number} | {a.Type} | {a.Name}")
        );

        var examplesSection = examples.Count == 0
            ? "Keine bisherigen Buchungen verfügbar."
            : string.Join(
                Environment.NewLine,
                examples
                    .Take(options.MaxExamples)
                    .Select(e => $"- \"{e.Description}\" ({e.Amount:N2}) -> {e.AccountShortName}")
            );

        return $$"""
        Du bist ein Buchhaltungs-Assistent für ein Schweizer Privat-Buchhaltungssystem (doppelte Buchführung).
        Aufgabe: Schlage für die folgende Bank-Transaktion die {{options.TopN}} wahrscheinlichsten Gegen-Konten vor.

        Antworte ausschliesslich mit einem JSON-Array gemäss Schema. Verwende ausschliesslich `accountShortName`-Werte, die im Kontoplan unten vorkommen.

        Kontoplan (ShortName | Nummer | Typ | Name):
        {{planLines}}

        Beispiele aus bisherigen Buchungen (Beschreibung -> ShortName):
        {{examplesSection}}

        Neue Transaktion:
        - Beschreibung: {{description}}
        - Betrag: {{amount:N2}}

        Liefere die Top {{options.TopN}} Vorschläge sortiert nach absteigender Confidence (0..1).
        """;
    }

    private object BuildResponseSchema() => new
    {
        type = "ARRAY",
        items = new
        {
            type = "OBJECT",
            properties = new
            {
                accountShortName = new { type = "STRING" },
                confidence = new { type = "NUMBER" },
            },
            required = new[] { "accountShortName", "confidence" },
        },
    };

    private record GeminiRequest
    {
        public List<GeminiContent> Contents { get; init; } = [];
        public GeminiGenerationConfig? GenerationConfig { get; init; }
    }

    private record GeminiContent
    {
        public List<GeminiPart> Parts { get; init; } = [];
    }

    private record GeminiPart
    {
        public string? Text { get; init; }
    }

    private record GeminiGenerationConfig
    {
        public string? ResponseMimeType { get; init; }
        public object? ResponseSchema { get; init; }
        public double? Temperature { get; init; }
    }

    private record GeminiResponse
    {
        public List<GeminiCandidate>? Candidates { get; init; }
    }

    private record GeminiCandidate
    {
        public GeminiContent? Content { get; init; }
    }
}

public static class GeminiServiceCollectionExtensions
{
    public static IServiceCollection AddGemini(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var options = new GeminiOptions();
        configuration.GetSection("Gemini").Bind(options);
        services.AddSingleton(options);
        services.AddHttpClient<IGeminiClient, GeminiClient>();
        return services;
    }
}
