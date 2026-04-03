namespace Kaesseli.Client.Blazor.Models;

public record AccountingPeriodDto(Guid Id, string Description, DateOnly FromInclusive, DateOnly ToInclusive);
