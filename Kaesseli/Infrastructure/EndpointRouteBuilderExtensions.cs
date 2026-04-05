using Kaesseli.Features.Accounts;
using Kaesseli.Features.Automation;
using Kaesseli.Features.Budget;
using Kaesseli.Features.Integration;
using Kaesseli.Features.Journal;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

public static class EndpointRouteBuilderExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IEndpointRouteBuilder MapKaesseliEndpoints(this IEndpointRouteBuilder app) =>
        app.MapBudgetEndpoints()
           .MapJournalEndpoints()
           .MapAccountEndpoints()
           .MapIntegrationEndpoints()
           .MapAutomationEndpoints();
}
