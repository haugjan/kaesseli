using Kaesseli.Domain.Journal;

namespace Kaesseli.Application.Integration;

public interface ICamtProcessor
{
    Task<IEnumerable<CamtEntry>> ReadCamtFile(string content, Guid accountId, CancellationToken cancellationToken);
}