using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kaesseli.Domain.Budget;
using Kaesseli.Domain.Common;

namespace Kaesseli.Infrastructure.Budget
{
    public class BudgetRepository : IBudgetRepository
    {
        public Task<BudgetEntry> AddBudgetEntry(BudgetEntry newBudgetEntryEntity)
        {
            throw new NotImplementedException();
        }

        public Task AssignAccount(Guid budgetId, Guid accountId)
        {
            throw new NotImplementedException();
        }

        public Task<Account> GetAccount(Guid accountId)
        {
            throw new NotImplementedException();
        }
    }
}
