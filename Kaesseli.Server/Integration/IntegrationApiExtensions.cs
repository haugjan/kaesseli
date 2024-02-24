using Kaesseli.Application.Integration;
using MediatR;

namespace Kaesseli.Server.Integration;

public static class IntegrationApiExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IEndpointRouteBuilder MapIntegrationEndpoints(this IEndpointRouteBuilder app) =>
        MapCamtApi(app);

    private static IEndpointRouteBuilder MapCamtApi(IEndpointRouteBuilder app)
    {
        app.MapPost(
            pattern: "/camt",
            async (IMediator mediator, ProcessCamtFileCommand command) =>
            {
                var guid = await mediator.Send(command);
                return Results.Created(uri: $"/camt/{guid}", guid);
            });
        return app;
    }
}