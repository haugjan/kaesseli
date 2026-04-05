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

            return app;
        }
    }
}
