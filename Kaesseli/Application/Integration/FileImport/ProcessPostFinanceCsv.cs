namespace Kaesseli.Application.Integration.FileImport;

public static class ProcessPostFinanceCsv
{
    public record Query(Stream Content, Guid AccountId);

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler : IHandler
    {
        public Task<Guid> Handle(Query request, CancellationToken cancellationToken) =>
            throw new NotImplementedException();
    }
}
