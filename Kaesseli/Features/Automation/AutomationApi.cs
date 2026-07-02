using Kaesseli.Features.Automation;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

public static class AutomationApi
{
    extension(IEndpointRouteBuilder app)
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public IEndpointRouteBuilder MapAutomationEndpoints()
        {
            app.MapGet(
                pattern: "/automation/nrMatchInput",
                async (GetNrOfPossibleAutomation.IHandler handler, [FromQuery] string input, CancellationToken ct) =>
                    await handler.Handle(new GetNrOfPossibleAutomation.Query(input), ct)
            );

            app.MapPost(
                pattern: "/automation",
                async (AddAutomation.IHandler handler, AddAutomation.Query command, CancellationToken ct) =>
                {
                    var guid = await handler.Handle(command, ct);
                    return Results.Created(uri: $"/automation/{guid}", guid);
                }
            );

            app.MapGet(
                pattern: "/automation",
                async (GetAutomations.IHandler handler, CancellationToken ct) =>
                    await handler.Handle(ct)
            );

            app.MapPut(
                pattern: "/automation/{id}",
                async (
                    UpdateAutomation.IHandler handler,
                    Guid id,
                    UpdateAutomation.Query command,
                    CancellationToken ct
                ) =>
                {
                    await handler.Handle(command with { Id = id }, ct);
                    return Results.NoContent();
                }
            );

            app.MapDelete(
                pattern: "/automation/{id}",
                async (DeleteAutomation.IHandler handler, Guid id, CancellationToken ct) =>
                {
                    await handler.Handle(new DeleteAutomation.Query(id), ct);
                    return Results.NoContent();
                }
            );

            return app;
        }
    }
}
