using System.Net.Http.Json;

namespace Kaesseli.Client.Blazor.Services;

public class KaesseliApiService(HttpClient httpClient)
{
    public Task<IEnumerable<AccountingPeriod>?> GetAccountingPeriodsAsync() =>
        httpClient.GetFromJsonAsync<IEnumerable<AccountingPeriod>>("accountingPeriod");

    public Task<FinancialOverview?> GetOverviewAsync(Guid periodId) =>
        httpClient.GetFromJsonAsync<FinancialOverview>($"accountingPeriod/{periodId}/overView");

    public Task<IEnumerable<AccountOverview>?> GetAccountSummariesAsync(Guid periodId) =>
        httpClient.GetFromJsonAsync<IEnumerable<AccountOverview>>(
            $"accountingPeriod/{periodId}/accountSummary"
        );

    public Task<AccountStatement?> GetAccountStatementAsync(Guid periodId, Guid accountId) =>
        httpClient.GetFromJsonAsync<AccountStatement>(
            $"accountingPeriod/{periodId}/account/{accountId}"
        );

    public Task<IEnumerable<TransactionSummary>?> GetTransactionSummariesAsync() =>
        httpClient.GetFromJsonAsync<IEnumerable<TransactionSummary>>("transactionSummary");

    public Task<IEnumerable<Transaction>?> GetTransactionsAsync(Guid transactionSummaryId) =>
        httpClient.GetFromJsonAsync<IEnumerable<Transaction>>(
            $"transaction?transactionSummaryId={transactionSummaryId}"
        );

    public Task<OpenTransaction?> GetNextOpenTransactionAsync(int skip = 0) =>
        httpClient.GetFromJsonAsync<OpenTransaction>($"transaction/nextOpen?skip={skip}");

    public Task AssignTransactionAsync(
        Guid accountingPeriodId,
        Guid transactionId,
        Guid otherAccountId
    ) =>
        httpClient.PatchAsJsonAsync(
            "transaction/journalEntry",
            new
            {
                accountingPeriodId,
                transactionId,
                otherAccountId,
            }
        );

    public Task SplitTransactionAsync(
        Guid accountingPeriodId,
        Guid transactionId,
        IEnumerable<SplitEntry> entries
    ) =>
        httpClient.PatchAsJsonAsync(
            "transaction/journalEntry/split",
            new
            {
                accountingPeriodId,
                transactionId,
                entries,
            }
        );

    public Task AddAutomationAsync(
        string automationText,
        Guid accountingPeriodId,
        IEnumerable<SplitEntry> entries
    ) =>
        httpClient.PostAsJsonAsync(
            "automation",
            new
            {
                automationText,
                accountingPeriodId,
                entries,
            }
        );

    public async Task<int> GetNrOfPossibleAutomationsAsync(string input)
    {
        var result = await httpClient.GetFromJsonAsync<NrOfPossibleAutomationResult>(
            $"automation/nrMatchInput?input={Uri.EscapeDataString(input)}"
        );
        return result?.NrOfPossibleAutomation ?? 0;
    }

    public Task<IEnumerable<Contracts.Accounts.Account>?> GetAccountsAsync(
        AccountType? accountType = null
    ) =>
        httpClient.GetFromJsonAsync<IEnumerable<Contracts.Accounts.Account>>(
            accountType.HasValue ? $"account?accountType={(int)accountType}" : "account"
        );

    public async Task UploadFileAsync(
        Stream fileStream,
        string fileName,
        Guid accountId,
        Guid accountingPeriodId
    )
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", fileName);
        content.Add(new StringContent(accountId.ToString()), "accountId");
        content.Add(new StringContent(accountingPeriodId.ToString()), "accountingPeriodId");
        var response = await httpClient.PostAsync("file/upload", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task AddAccountingPeriodAsync(
        string description,
        DateOnly fromInclusive,
        DateOnly toInclusive,
        CancellationToken ct = default
    )
    {
        var response = await httpClient.PostAsJsonAsync(
            "accountingPeriod",
            new
            {
                fromInclusive,
                toInclusive,
                description,
            },
            ct
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAccountingPeriodAsync(
        Guid id,
        string description,
        DateOnly fromInclusive,
        DateOnly toInclusive,
        CancellationToken ct = default
    )
    {
        var response = await httpClient.PutAsJsonAsync(
            $"accountingPeriod/{id}",
            new
            {
                id,
                description,
                fromInclusive,
                toInclusive,
            },
            ct
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAccountingPeriodAsync(Guid id, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"accountingPeriod/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    public Task<IEnumerable<Contracts.Budget.BudgetEntry>?> GetBudgetEntriesAsync(
        Guid accountingPeriodId,
        CancellationToken ct = default
    ) =>
        httpClient.GetFromJsonAsync<IEnumerable<Contracts.Budget.BudgetEntry>>(
            $"budgetEntry?accountingPeriodId={accountingPeriodId}",
            ct
        );

    public async Task SetBudgetAsync(
        decimal amount,
        string description,
        Guid accountId,
        Guid accountingPeriodId,
        CancellationToken ct = default
    )
    {
        var response = await httpClient.PostAsJsonAsync(
            "budgetEntry",
            new
            {
                amount,
                description,
                accountId,
                accountingPeriodId,
            },
            ct
        );
        response.EnsureSuccessStatusCode();
    }

    public Task<IEnumerable<Contracts.Journal.JournalEntry>?> GetJournalEntriesAsync(
        Guid accountingPeriodId,
        AccountType? accountType = null,
        CancellationToken ct = default
    )
    {
        var url = $"journalEntry?accountingPeriodId={accountingPeriodId}";
        if (accountType.HasValue)
            url += $"&accountType={accountType}";
        return httpClient.GetFromJsonAsync<IEnumerable<Contracts.Journal.JournalEntry>>(url, ct);
    }

    public async Task AddOpeningBalanceAsync(
        decimal amount,
        string description,
        Guid debitAccountId,
        Guid creditAccountId,
        Guid accountingPeriodId,
        CancellationToken ct = default
    )
    {
        var response = await httpClient.PostAsJsonAsync(
            "journalEntry/openingBalance",
            new
            {
                amount,
                description,
                debitAccountId,
                creditAccountId,
                accountingPeriodId,
            },
            ct
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task AddAccountAsync(
        string name,
        AccountType type,
        string number,
        string shortName,
        string icon,
        string iconColor,
        CancellationToken ct = default
    )
    {
        var response = await httpClient.PostAsJsonAsync(
            "account",
            new
            {
                name,
                type,
                number,
                shortName,
                icon,
                iconColor,
            },
            ct
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task UpdateAccountAsync(
        Guid id,
        string name,
        AccountType type,
        string number,
        string shortName,
        string icon,
        string iconColor,
        CancellationToken ct = default
    )
    {
        var response = await httpClient.PutAsJsonAsync(
            $"account/{id}",
            new
            {
                id,
                name,
                type,
                number,
                shortName,
                icon,
                iconColor,
            },
            ct
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAccountAsync(Guid id, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"account/{id}", ct);
        response.EnsureSuccessStatusCode();
    }

    public record SplitEntry(Guid OtherAccountId, decimal Amount);

    private record NrOfPossibleAutomationResult(int NrOfPossibleAutomation);
}
