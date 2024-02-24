namespace Kaesseli.Application.Integration;

public interface ICamtProcessor
{
    Task<CamtDocument> ReadCamtFile(string content, CancellationToken cancellationToken);
}