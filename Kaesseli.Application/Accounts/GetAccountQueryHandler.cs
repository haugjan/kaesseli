using System.Collections.Immutable;
using Kaesseli.Application.Utility;
using Kaesseli.Domain.Accounts;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Journal;
using MediatR;

namespace Kaesseli.Application.Accounts;

// ReSharper disable once UnusedType.Global
public class GetAccountQueryHandler(IAccountRepository accountRepo, IJournalRepository journalRepo, IBudgetRepository budgetRepo,
                                    IDateTimeService dateTimeService)
    : IRequestHandler<GetAccountQuery, GetAccountQueryResult>
{
    private readonly IDateTimeService _dateTimeService = dateTimeService;

    public async Task<GetAccountQueryResult> Handle(GetAccountQuery request, CancellationToken cancellationToken)
    {
        var account = await accountRepo.GetAccount(request.AccountId, cancellationToken);
        var period = await accountRepo.GetAccountingPeriod(request.AccountingPeriodId, cancellationToken);
        var journalEntries = (await journalRepo.GetJournalEntries(
                                  request: new GetJournalEntriesRequest
                                  {
                                      AccountId = request.AccountId, AccountingPeriodId = request.AccountingPeriodId
                                  },
                                  cancellationToken)).ToArray();
        var budgetEntries = (await budgetRepo.GetBudgetEntries(
                                 request: new GetBudgetEntriesRequest
                                 {
                                     AccountingPeriodId = request.AccountingPeriodId, AccountId = request.AccountId
                                 },
                                 cancellationToken)).ToArray();

        var accountBalance = account.GetAccountBalance(journalEntries);
        var budget = account.GetBudget(budgetEntries);
        var currentBudget = account.GetCurrentBudget(budgetEntries, period, _dateTimeService.ToDay);
        var budgetBalance = account.GetBudgetBalance(currentBudget, accountBalance);

        return new GetAccountQueryResult
        {
            Id = account.Id,
            Name = account.Name,
            Icon = account.Icon,
            Type = account.Type.DisplayName(),
            TypeId = account.Type,
            AccountBalance = accountBalance,
            Budget = budget,
            BudgetBalance = budgetBalance,
            Entries = GetEntries(
                account.Id,
                journalEntries,
                budgetEntries),
            IconColor = account.IconColor,
            CurrentBudget = currentBudget
        };
    }

    private static IEnumerable<GetAccountQueryResultEntry> GetEntries(
        Guid accountId,
        IEnumerable<JournalEntry> journalEntries,
        IEnumerable<BudgetEntry> budgetEntries)
    {
        var journalResults = journalEntries.Select(entry => CreateAccountQueryResultEntry(accountId, entry));

        var budgetResults = budgetEntries.Select(CreateAccountQueryResultEntry);

        return journalResults.Concat(budgetResults)
                             .OrderBy(entry => entry.ValueDate)
                             .ThenBy(entry => entry.AmountType)
                             .ThenBy(entry => entry.Amount)
                             .ToImmutableList();
    }

    private static GetAccountQueryResultEntry CreateAccountQueryResultEntry(BudgetEntry entry) =>
        new()
        {
            Id = entry.Id,
            ValueDate = entry.AccountingPeriod.FromInclusive,
            Description = entry.Description,
            Amount = entry.Amount,
            AmountType = AmountType.Budget,
            OtherAccount = null,
            OtherAccountId = null
        };

    private static GetAccountQueryResultEntry CreateAccountQueryResultEntry(Guid accountId, JournalEntry entry)
    {
        var account = entry.CreditAccount.Id == accountId
                          ? entry.CreditAccount
                          : entry.DebitAccount;
        var otherAccount = entry.CreditAccount.Id == accountId
                               ? entry.DebitAccount
                               : entry.CreditAccount;
        var isDebit = entry.DebitAccount.Id == accountId;
        var amount = account.GetSignedAmount(entry);

        return new GetAccountQueryResultEntry
        {
            Id = entry.Id,
            ValueDate = entry.ValueDate,
            Description = entry.Description,
            Amount = amount,
            AmountType = isDebit ? AmountType.Debit : AmountType.Credit,
            OtherAccount = otherAccount.Name,
            OtherAccountId = otherAccount.Id
        };
    }
}