using Kaesseli.Client.Blazor.Models;

namespace Kaesseli.Client.Blazor.Services;

public class AccountingPeriodState
{
    public IReadOnlyList<AccountingPeriodDto> Periods { get; private set; } = [];
    public Guid? SelectedPeriodId { get; private set; }
    public event Action? OnChange;

    public void Initialize(IReadOnlyList<AccountingPeriodDto> periods, Guid? savedId)
    {
        Periods = periods;
        SelectedPeriodId = savedId.HasValue && periods.Any(p => p.Id == savedId)
            ? savedId
            : periods.LastOrDefault()?.Id;
        OnChange?.Invoke();
    }

    public void SelectPeriod(Guid id)
    {
        SelectedPeriodId = id;
        OnChange?.Invoke();
    }
}
