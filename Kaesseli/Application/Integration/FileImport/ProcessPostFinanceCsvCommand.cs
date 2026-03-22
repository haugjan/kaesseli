namespace Kaesseli.Application.Integration.FileImport;

public class ProcessPostFinanceCsvCommand
{
    public required Stream Content { get; init; }
    public required Guid AccountId { get; init; }
}