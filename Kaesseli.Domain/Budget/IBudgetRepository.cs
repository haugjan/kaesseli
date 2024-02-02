using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kaesseli.Domain.Common;

namespace Kaesseli.Domain.Budget;

public interface IBudgetRepository
{
    Task<BudgetEntry> AddBudgetEntry(BudgetEntry newBudgetEntryEntity, CancellationToken ct);
    Task<Account> GetAccount(Guid accountId, CancellationToken ct);

    Task<IEnumerable<BudgetEntry>> GetBudgetEntries(
        GetBudgetEntriesRequest request,
        CancellationToken cancellationToken);

    Task<Account> AddAccount(Account account, CancellationToken cancellationToken);
}