namespace Kaesseli.Application.Integration;

public interface ICamtProcessor
{
    Task<CamtDocument> ReadCamtFile(Stream content, CancellationToken cancellationToken);
}