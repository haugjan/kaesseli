using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kässeli.Domain.Entities;

namespace Kässeli.Domain.Repositories;

public interface IBudgetRepository
{
    Task<BudgetEntry> AddBudgetEntry(BudgetEntry newBudgetEntryEntity);
    Task AssignAccount(Guid budgetId, Guid accountId);
    Task<Account> GetAccount(Guid accountId);
}