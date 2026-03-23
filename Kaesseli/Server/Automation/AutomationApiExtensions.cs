using Kaesseli.Application.Automation;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;


public static class AutomationApiExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IEndpointRouteBuilder MapAutomationEndpoints(this IEndpointRouteBuilder app) =>
        MapCamtApi(app);

    private static IEndpointRouteBuilder MapCamtApi(IEndpointRouteBuilder app)
    {
        app.MapGet(
            pattern: "/automation/nrMatchInput",
            async (GetNrOfPossibleAutomation.IHandler handler, [FromQuery] string input) =>
                await handler.Handle(new GetNrOfPossibleAutomation.Query { AutomationText = input }, default));

        app.MapPost(
            pattern: "/automation",
            async (AddAutomation.IHandler handler, AddAutomation.Query addAutomationCommand) =>
            {
                var guid = await handler.Handle(addAutomationCommand, default);
                return Results.Created(uri: $"/automation/{guid}", guid);
            });

        return app;
    }
}
