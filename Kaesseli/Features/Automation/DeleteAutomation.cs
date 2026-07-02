namespace Kaesseli.Features.Automation;

public static class DeleteAutomation
{
    public record Query(Guid Id);

    public interface IHandler
    {
        Task Handle(Query request, CancellationToken cancellationToken);
    }

    public class Handler(IAutomationRepository repository) : IHandler
    {
        public async Task Handle(Query request, CancellationToken cancellationToken) =>
            await repository.DeleteAutomation(request.Id, cancellationToken);
    }
}
