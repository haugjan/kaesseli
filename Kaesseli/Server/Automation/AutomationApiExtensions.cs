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
            async (IGetNrOfPossibleAutomationQueryHandler handler, [FromQuery] string input) =>
                await handler.Handle(new GetNrOfPossibleAutomationQuery { AutomationText = input }, default));

        app.MapPost(
            pattern: "/automation",
            async (IAddAutomationCommandHandler handler, AddAutomationCommand addAutomationCommand) =>
            {
                var guid = await handler.Handle(addAutomationCommand, default);
                return Results.Created(uri: $"/automation/{guid}", guid);
            });

        return app;
    }
}
