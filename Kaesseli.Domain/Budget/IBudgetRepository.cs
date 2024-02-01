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
    Task AssignAccount(Guid budgetId, Guid accountId, CancellationToken ct);
    Task<Account> GetAccount(Guid accountId, CancellationToken ct);
}