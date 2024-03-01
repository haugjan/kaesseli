namespace Kaesseli.Application.Integration.Camt;

public interface ICamtProcessor
{
    Task<CamtDocument> ReadCamtFile(Stream content, CancellationToken cancellationToken);
}