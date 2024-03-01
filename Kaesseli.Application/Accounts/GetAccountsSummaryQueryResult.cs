using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public class GetAccountsSummaryQueryResult
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Guid Id { get; init; }
    public required string Name{ get; init; }
    public required string Icon { get; set; }
    public required string IconColor { get; set; }
    public required string Type { get; init; }
    public required AccountType TypeId { get; init; }
    // ReSharper disable once MemberCanBePrivate.Global
    public ParentAccountType ParentTypeId => TypeId.ToParentAccountType();
    // ReSharper disable once UnusedMember.Global
    public string ParentType => ParentTypeId.DisplayName();
    public required decimal AccountBalance{ get; init; }
    public required decimal? Budget{ get; init; }
    public required decimal? BudgetBalance{ get; init; }
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}