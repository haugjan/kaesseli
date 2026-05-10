using Kaesseli.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Kaesseli.Features.AccountSuggestion;

public interface IAccountSuggestionRepository
{
    Task<AccountSuggestion?> GetByTransactionId(Guid transactionId, CancellationToken cancellationToken);
    Task<HashSet<Guid>> GetTransactionIdsWithSuggestion(CancellationToken cancellationToken);
    Task Add(AccountSuggestion suggestion, CancellationToken cancellationToken);
}

internal class AccountSuggestionRepository(KaesseliContext context) : IAccountSuggestionRepository
{
    public async Task<AccountSuggestion?> GetByTransactionId(
        Guid transactionId,
        CancellationToken cancellationToken
    )
    {
        var all = await context.AccountSuggestions.ToListAsync(cancellationToken);
        return all.FirstOrDefault(s => s.TransactionId == transactionId);
    }

    public async Task<HashSet<Guid>> GetTransactionIdsWithSuggestion(
        CancellationToken cancellationToken
    )
    {
        var all = await context.AccountSuggestions.ToListAsync(cancellationToken);
        return all.Select(s => s.TransactionId).ToHashSet();
    }

    public async Task Add(AccountSuggestion suggestion, CancellationToken cancellationToken)
    {
        context.AccountSuggestions.Add(suggestion);
        await context.SaveChangesAsync(cancellationToken);
    }
}
