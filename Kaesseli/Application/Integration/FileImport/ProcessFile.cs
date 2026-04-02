using Kaesseli.Application.Automation;

namespace Kaesseli.Application.Integration.FileImport;

public static class ProcessFile
{
    public record Query(FileType FileType, Stream Content, Guid AccountId, Guid AccountingPeriodId);

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(
        ProcessCamtFile.IHandler camtHandler,
        ProcessPostFinanceCsv.IHandler postFinanceCsvHandler,
        ApplyAllAutomations.IHandler applyAllAutomationsHandler) : IHandler
    {
        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = request.FileType switch
            {
                FileType.Camt => await camtHandler.Handle(
                                     request: new ProcessCamtFile.Query(Content: request.Content, AccountId: request.AccountId),
                                     cancellationToken),
                FileType.PostFinanceCsv => await postFinanceCsvHandler.Handle(
                                               request: new ProcessPostFinanceCsv.Query(Content: request.Content, AccountId: request.AccountId),
                                               cancellationToken),
                _ => throw new NotImplementedException()
            };
            await applyAllAutomationsHandler.Handle(
                request: new ApplyAllAutomations.Query(request.AccountingPeriodId),
                cancellationToken);
            return result;
        }
    }
}
