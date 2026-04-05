using System.Net.Http.Json;

namespace Kaesseli.Client.Blazor.Services;

public class KaesseliApiService(HttpClient httpClient)
{
    public Task<IEnumerable<AccountingPeriod>?> GetAccountingPeriodsAsync()
        => httpClient.GetFromJsonAsync<IEnumerable<AccountingPeriod>>("accountingPeriod");

    public Task<FinancialOverview?> GetOverviewAsync(Guid periodId)
        => httpClient.GetFromJsonAsync<FinancialOverview>($"accountingPeriod/{periodId}/overView");

    public Task<IEnumerable<AccountOverview>?> GetAccountSummariesAsync(Guid periodId)
        => httpClient.GetFromJsonAsync<IEnumerable<AccountOverview>>($"accountingPeriod/{periodId}/accountSummary");

    public Task<AccountStatement?> GetAccountStatementAsync(Guid periodId, Guid accountId)
        => httpClient.GetFromJsonAsync<AccountStatement>($"accountingPeriod/{periodId}/account/{accountId}");

    public Task<IEnumerable<TransactionSummary>?> GetTransactionSummariesAsync()
        => httpClient.GetFromJsonAsync<IEnumerable<TransactionSummary>>("transactionSummary");

    public Task<IEnumerable<Transaction>?> GetTransactionsAsync(Guid transactionSummaryId)
        => httpClient.GetFromJsonAsync<IEnumerable<Transaction>>($"transaction?transactionSummaryId={transactionSummaryId}");

    public Task<OpenTransaction?> GetNextOpenTransactionAsync(int skip = 0)
        => httpClient.GetFromJsonAsync<OpenTransaction>($"transaction/nextOpen?skip={skip}");

    public Task AssignTransactionAsync(Guid accountingPeriodId, Guid transactionId, Guid otherAccountId)
        => httpClient.PatchAsJsonAsync("transaction/journalEntry", new { accountingPeriodId, transactionId, otherAccountId });

    public Task SplitTransactionAsync(Guid accountingPeriodId, Guid transactionId, IEnumerable<SplitEntry> entries)
        => httpClient.PatchAsJsonAsync("transaction/journalEntry/split", new { accountingPeriodId, transactionId, entries });

    public Task AddAutomationAsync(string automationText, Guid accountingPeriodId, IEnumerable<SplitEntry> entries)
        => httpClient.PostAsJsonAsync("automation", new { automationText, accountingPeriodId, entries });

    public async Task<int> GetNrOfPossibleAutomationsAsync(string input)
    {
        var result = await httpClient.GetFromJsonAsync<NrOfPossibleAutomationResult>($"automation/nrMatchInput?input={Uri.EscapeDataString(input)}");
        return result?.NrOfPossibleAutomation ?? 0;
    }

    public Task<IEnumerable<Contracts.Accounts.Account>?> GetAccountsAsync(AccountType? accountType = null)
        => httpClient.GetFromJsonAsync<IEnumerable<Contracts.Accounts.Account>>(
            accountType.HasValue ? $"account?accountType={(int)accountType}" : "account");

    public async Task UploadFileAsync(Stream fileStream, string fileName, Guid accountId, Guid accountingPeriodId)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", fileName);
        content.Add(new StringContent(accountId.ToString()), "accountId");
        content.Add(new StringContent(accountingPeriodId.ToString()), "accountingPeriodId");
        var response = await httpClient.PostAsync("file/upload", content);
        response.EnsureSuccessStatusCode();
    }

    public record SplitEntry(Guid OtherAccountId, decimal Amount);
    private record NrOfPossibleAutomationResult(int NrOfPossibleAutomation);
}
