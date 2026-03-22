namespace Kaesseli.Application.Integration.FileImport;

public interface IProcessPostFinanceCsvCommandHandler
{
    Task<Guid> Handle(ProcessPostFinanceCsvCommand request, CancellationToken cancellationToken);
}

public class ProcessPostFinanceCsvCommandHandler : IProcessPostFinanceCsvCommandHandler
{
    public Task<Guid> Handle(ProcessPostFinanceCsvCommand request, CancellationToken cancellationToken) =>
        throw new NotImplementedException();
}
