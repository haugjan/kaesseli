namespace Kaesseli.Application.Integration.NextOpenTransaction;

// ReSharper disable once ClassNeverInstantiated.Global
public record SplitOpenTransactionEntry(Guid OtherAccountId, decimal Amount);
