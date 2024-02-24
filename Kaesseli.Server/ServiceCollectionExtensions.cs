using Kaesseli.Server.Accounts;
using Kaesseli.Server.Budget;
using Kaesseli.Server.Integration;
using Kaesseli.Server.Journal;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

public static class EndpointRouteBuilderExtensions
{
    public static IEndpointRouteBuilder MapKaesseliEndpoints(this IEndpointRouteBuilder app) =>
        app.MapBudgetEndpoints()
           .MapJournalEndpoints()
           .MapAccountEndpoints()
           .MapIntegrationEndpoints();
}