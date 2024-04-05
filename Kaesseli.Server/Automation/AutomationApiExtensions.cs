using Kaesseli.Application.Automation;
using Kaesseli.Application.Integration.TransactionQuery;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;

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
            async (IMediator mediator, [FromQuery] string input) =>
            {
                var query = new GetNrOfPossibleAutomationQuery { AutomationText = input };
                return await mediator.Send(query);
            });

        app.MapPost(
            pattern: "/automation",
            async (IMediator mediator, AddAutomationCommand addAutomationCommand) =>
            {
                var guid = await mediator.Send(addAutomationCommand);
                return Results.Created(uri: $"/automation/{guid}", guid);
            });

        return app;
    }
}