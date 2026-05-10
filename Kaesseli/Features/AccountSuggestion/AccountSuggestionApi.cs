using Kaesseli.Features.AccountSuggestion;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

public static class AccountSuggestionApi
{
    extension(IEndpointRouteBuilder app)
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public IEndpointRouteBuilder MapAccountSuggestionEndpoints()
        {
            app.MapPost(
                "/accountSuggestion/generate",
                (GenerateAccountSuggestions.IRunner runner) =>
                {
                    var started = runner.TryStartInBackground();
                    return Results.Json(MapStatus(runner.Status, started), statusCode: started ? 202 : 200);
                }
            );

            app.MapGet(
                "/accountSuggestion/status",
                (GenerateAccountSuggestions.IRunner runner) =>
                    Results.Ok(MapStatus(runner.Status, started: null))
            );

            return app;
        }
    }

    private static Contracts.AccountSuggestion.AccountSuggestionJobStatus MapStatus(
        AccountSuggestionJobStatus status,
        bool? started
    ) => new(
        IsRunning: status.IsRunning,
        Started: started,
        RunId: status.RunId,
        StartedAt: status.StartedAt,
        FinishedAt: status.FinishedAt,
        Total: status.Total,
        Processed: status.Processed,
        Failed: status.Failed,
        LastError: status.LastError
    );
}
