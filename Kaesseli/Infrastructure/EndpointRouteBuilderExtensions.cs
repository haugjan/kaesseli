using Kaesseli.Features.Accounts;
using Kaesseli.Features.Automation;
using Kaesseli.Features.Budget;
using Kaesseli.Features.Integration;
using Kaesseli.Features.Journal;

// ReSharper disable once CheckNamespace
namespace Microsoft.AspNetCore.Routing;

public static class EndpointRouteBuilderExtensions
{
    extension(IEndpointRouteBuilder app)
    {
        // ReSharper disable once UnusedMethodReturnValue.Global
        public IEndpointRouteBuilder MapKaesseliEndpoints() =>
            app.MapBudgetEndpoints()
               .MapJournalEndpoints()
               .MapAccountEndpoints()
               .MapIntegrationEndpoints()
               .MapAutomationEndpoints();
    }
}
