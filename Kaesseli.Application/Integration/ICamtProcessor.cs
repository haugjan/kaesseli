using Kaesseli.Domain.Journal;

namespace Kaesseli.Application.Integration;

public interface ICamtProcessor
{
    Task<CamtDocument> ReadCamtFile(string content, Guid accountId, CancellationToken cancellationToken);
}