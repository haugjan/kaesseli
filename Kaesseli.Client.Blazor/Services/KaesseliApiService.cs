using System.Net.Http.Json;
using Kaesseli.Client.Blazor.Models;

namespace Kaesseli.Client.Blazor.Services;

public class KaesseliApiService(HttpClient httpClient)
{
    public Task<IEnumerable<AccountingPeriodDto>?> GetAccountingPeriodsAsync(CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<IEnumerable<AccountingPeriodDto>>("accountingPeriod", ct);

    public Task<FinancialOverviewDto?> GetOverviewAsync(Guid periodId, CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<FinancialOverviewDto>($"accountingPeriod/{periodId}/overView", ct);

    public Task<IEnumerable<AccountSummaryDto>?> GetAccountSummariesAsync(Guid periodId, CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<IEnumerable<AccountSummaryDto>>($"accountingPeriod/{periodId}/accountSummary", ct);
}
