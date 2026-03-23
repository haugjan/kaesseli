namespace Kaesseli.Application.Integration.FileImport;

public static class ProcessPostFinanceCsv
{
    public record Query
    {
        public required Stream Content { get; init; }
        public required Guid AccountId { get; init; }
    }

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
