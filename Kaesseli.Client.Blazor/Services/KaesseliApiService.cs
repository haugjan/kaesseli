using System.Net.Http.Json;


namespace Kaesseli.Client.Blazor.Services;

public class KaesseliApiService(HttpClient httpClient)
{
    public Task<IEnumerable<AccountingPeriod>?> GetAccountingPeriodsAsync(CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<IEnumerable<AccountingPeriod>>("accountingPeriod", ct);

    public Task<FinancialOverview?> GetOverviewAsync(Guid periodId, CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<FinancialOverview>($"accountingPeriod/{periodId}/overView", ct);

    public Task<IEnumerable<AccountOverview>?> GetAccountSummariesAsync(Guid periodId, CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<IEnumerable<AccountOverview>>($"accountingPeriod/{periodId}/accountSummary", ct);

    public Task<AccountStatement?> GetAccountStatementAsync(Guid periodId, Guid accountId, CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<AccountStatement>($"accountingPeriod/{periodId}/account/{accountId}", ct);

    public Task<IEnumerable<TransactionSummary>?> GetTransactionSummariesAsync(CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<IEnumerable<TransactionSummary>>("transactionSummary", ct);

    public Task<IEnumerable<Transaction>?> GetTransactionsAsync(Guid transactionSummaryId, CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<IEnumerable<Transaction>>($"transaction?transactionSummaryId={transactionSummaryId}", ct);

    public Task<OpenTransaction?> GetNextOpenTransactionAsync(int skip = 0, CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<OpenTransaction>($"transaction/nextOpen?skip={skip}", ct);

    public Task AssignTransactionAsync(Guid accountingPeriodId, Guid transactionId, Guid otherAccountId, CancellationToken ct = default)
        => httpClient.PatchAsJsonAsync("transaction/journalEntry", new { accountingPeriodId, transactionId, otherAccountId }, ct);

    public Task SplitTransactionAsync(Guid accountingPeriodId, Guid transactionId, IEnumerable<SplitEntry> entries, CancellationToken ct = default)
        => httpClient.PatchAsJsonAsync("transaction/journalEntry/split", new { accountingPeriodId, transactionId, entries }, ct);

    public Task AddAutomationAsync(string automationText, Guid accountingPeriodId, IEnumerable<SplitEntry> entries, CancellationToken ct = default)
        => httpClient.PostAsJsonAsync("automation", new { automationText, accountingPeriodId, entries }, ct);

    public async Task<int> GetNrOfPossibleAutomationsAsync(string input, CancellationToken ct = default)
    {
        var result = await httpClient.GetFromJsonAsync<NrOfPossibleAutomationResult>($"automation/nrMatchInput?input={Uri.EscapeDataString(input)}", ct);
        return result?.NrOfPossibleAutomation ?? 0;
    }

    public Task<IEnumerable<Contracts.Accounts.Account>?> GetAccountsAsync(AccountType? accountType = null, CancellationToken ct = default)
        => httpClient.GetFromJsonAsync<IEnumerable<Contracts.Accounts.Account>>(
            accountType.HasValue ? $"account?accountType={(int)accountType}" : "account", ct);

    public async Task UploadFileAsync(Stream fileStream, string fileName, Guid accountId, Guid accountingPeriodId, CancellationToken ct = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", fileName);
        content.Add(new StringContent(accountId.ToString()), "accountId");
        content.Add(new StringContent(accountingPeriodId.ToString()), "accountingPeriodId");
        var response = await httpClient.PostAsync("file/upload", content, ct);
        response.EnsureSuccessStatusCode();
    }

    public record SplitEntry(Guid OtherAccountId, decimal Amount);
    private record NrOfPossibleAutomationResult(int NrOfPossibleAutomation);
}
