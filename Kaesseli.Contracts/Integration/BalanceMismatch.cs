namespace Kaesseli.Contracts.Integration;

public record BalanceMismatch(
    string FileName,
    decimal ExpectedDelta,
    decimal ActualDelta,
    decimal Difference,
    DateOnly ValueDateFrom,
    DateOnly ValueDateTo);

public record BalanceMismatchResponse(IReadOnlyList<BalanceMismatch> Mismatches);
