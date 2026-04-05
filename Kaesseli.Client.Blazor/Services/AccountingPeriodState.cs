

namespace Kaesseli.Client.Blazor.Services;

public class AccountingPeriodState
{
    public IReadOnlyList<AccountingPeriod> Periods { get; private set; } = [];
    public Guid? SelectedPeriodId { get; private set; }
    public event Action? OnChange;

    public void Initialize(IReadOnlyList<AccountingPeriod> periods, Guid? savedId)
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
