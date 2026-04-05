using System.Net.Http.Json;


namespace Kaesseli.Client.Blazor.Services;

public class KaesseliApiService(HttpClient httpClient)
{
    public Task<IEnumerable<AccountingPeriod>?> GetAccountingPeriodsAsync(CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<IEnumerable<AccountingPeriod>>("accountingPeriod", ct);

    public Task<FinancialOverview?> GetOverviewAsync(Guid periodId, CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<FinancialOverview>($"accountingPeriod/{periodId}/overView", ct);

    public Task<IEnumerable<AccountSummary>?> GetAccountSummariesAsync(Guid periodId, CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<IEnumerable<AccountSummary>>($"accountingPeriod/{periodId}/accountSummary", ct);

    public Task<AccountDetail?> GetAccountDetailAsync(Guid periodId, Guid accountId, CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<AccountDetail>($"accountingPeriod/{periodId}/account/{accountId}", ct);

    public Task<IEnumerable<TransactionSummary>?> GetTransactionSummariesAsync(CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<IEnumerable<TransactionSummary>>("transactionSummary", ct);

    public Task<IEnumerable<Transaction>?> GetTransactionsAsync(Guid transactionSummaryId, CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<IEnumerable<Transaction>>($"transaction?transactionSummaryId={transactionSummaryId}", ct);
}
