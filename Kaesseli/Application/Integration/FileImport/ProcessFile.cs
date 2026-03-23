using Kaesseli.Application.Automation;

namespace Kaesseli.Application.Integration.FileImport;

public static class ProcessFile
{
    public record Query
    {
        // ReSharper disable UnusedAutoPropertyAccessor.Global
        public required FileType FileType { get; init; }
        public required Stream Content { get; init; }
        public required Guid AccountId { get; init; }
        public required Guid AccountingPeriodId { get; init; }
        // ReSharper restore UnusedAutoPropertyAccessor.Global
    }

    public interface IHandler
    {
        Task<Guid> Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler : IHandler
    {
        private readonly ProcessCamtFile.IHandler _camtHandler;
        private readonly ProcessPostFinanceCsv.IHandler _postFinanceCsvHandler;
        private readonly ApplyAllAutomations.IHandler _applyAllAutomationsHandler;

        public Handler(
            ProcessCamtFile.IHandler camtHandler,
            ProcessPostFinanceCsv.IHandler postFinanceCsvHandler,
            ApplyAllAutomations.IHandler applyAllAutomationsHandler)
        {
            _camtHandler = camtHandler;
            _postFinanceCsvHandler = postFinanceCsvHandler;
            _applyAllAutomationsHandler = applyAllAutomationsHandler;
        }

        public async Task<Guid> Handle(Query request, CancellationToken cancellationToken)
        {
            var result = request.FileType switch
            {
                FileType.Camt => await _camtHandler.Handle(
                                     request: new ProcessCamtFile.Query { Content = request.Content, AccountId = request.AccountId },
                                     cancellationToken),
                FileType.PostFinanceCsv => await _postFinanceCsvHandler.Handle(
                                               request: new ProcessPostFinanceCsv.Query
                                               {
                                                   Content = request.Content, AccountId = request.AccountId
                                               },
                                               cancellationToken),
                _ => throw new NotImplementedException()
            };
            await _applyAllAutomationsHandler.Handle(
                request: new ApplyAllAutomations.Query { AccountingPeriodId = request.AccountingPeriodId },
                cancellationToken);
            return result;
        }
    }
}
