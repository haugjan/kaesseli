using Kaesseli.Application.Automation;
using Kaesseli.Domain.Integration;
using MediatR;

namespace Kaesseli.Application.Integration.FileImport;

public class ProcessFileCommandHandler : IRequestHandler<ProcessFileCommand, Guid>
{
    private readonly IMediator _mediator;

    public ProcessFileCommandHandler(IMediator mediator) =>
        _mediator = mediator;

    public async Task<Guid> Handle(ProcessFileCommand request, CancellationToken cancellationToken)
    {
        var result = request.FileType switch
        {
            FileType.Camt => await _mediator.Send(
                                 request: new ProcessCamtFileCommand { Content = request.Content, AccountId = request.AccountId },
                                 cancellationToken),
            FileType.PostFinanceCsv => await _mediator.Send(
                                           request: new ProcessPostFinanceCsvCommand
                                           {
                                               Content = request.Content, AccountId = request.AccountId
                                           },
                                           cancellationToken),
            _ => throw new NotImplementedException()
        };
        await _mediator.Send(
            request: new ApplyAllAutomationsCommand { AccountingPeriodId = request.AccountingPeriodId },
            cancellationToken);
        return result;
    }
}