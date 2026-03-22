using Kaesseli.Application.Automation;

namespace Kaesseli.Application.Integration.FileImport;

public interface IProcessFileCommandHandler
{
    Task<Guid> Handle(ProcessFileCommand request, CancellationToken cancellationToken);
}

public class ProcessFileCommandHandler : IProcessFileCommandHandler
{
    private readonly IProcessCamtFileCommandHandler _camtHandler;
    private readonly IProcessPostFinanceCsvCommandHandler _postFinanceCsvHandler;
    private readonly IApplyAllAutomationsCommandHandler _applyAllAutomationsHandler;

    public ProcessFileCommandHandler(
        IProcessCamtFileCommandHandler camtHandler,
        IProcessPostFinanceCsvCommandHandler postFinanceCsvHandler,
        IApplyAllAutomationsCommandHandler applyAllAutomationsHandler)
    {
        _camtHandler = camtHandler;
        _postFinanceCsvHandler = postFinanceCsvHandler;
        _applyAllAutomationsHandler = applyAllAutomationsHandler;
    }

    public async Task<Guid> Handle(ProcessFileCommand request, CancellationToken cancellationToken)
    {
        var result = request.FileType switch
        {
            FileType.Camt => await _camtHandler.Handle(
                                 request: new ProcessCamtFileCommand { Content = request.Content, AccountId = request.AccountId },
                                 cancellationToken),
            FileType.PostFinanceCsv => await _postFinanceCsvHandler.Handle(
                                           request: new ProcessPostFinanceCsvCommand
                                           {
                                               Content = request.Content, AccountId = request.AccountId
                                           },
                                           cancellationToken),
            _ => throw new NotImplementedException()
        };
        await _applyAllAutomationsHandler.Handle(
            request: new ApplyAllAutomationsCommand { AccountingPeriodId = request.AccountingPeriodId },
            cancellationToken);
        return result;
    }
}
