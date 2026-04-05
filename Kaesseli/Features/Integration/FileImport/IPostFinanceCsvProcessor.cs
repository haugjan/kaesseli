namespace Kaesseli.Features.Integration.FileImport;

public interface IPostFinanceCsvProcessor
{
    Task<FinancialDocument> ReadCsvFile(Stream content, CancellationToken cancellationToken);
}