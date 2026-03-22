namespace Kaesseli.Application.Integration.FileImport;

public interface ICamtProcessor
{
    Task<FinancialDocument> ReadCamtFile(Stream content, CancellationToken cancellationToken);
}