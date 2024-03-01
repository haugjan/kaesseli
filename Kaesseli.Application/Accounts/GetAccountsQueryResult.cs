using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

public class GetAccountsQueryResult
{
    // ReSharper disable UnusedAutoPropertyAccessor.Global
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string Type => TypeId.DisplayName();
    public required AccountType TypeId { get; init; }

    // ReSharper disable once UnusedMember.Global
    public string ParentType => ParentTypeId.DisplayName();

    // ReSharper disable once MemberCanBePrivate.Global
    public ParentAccountType ParentTypeId => TypeId.ToParentAccountType();
    // ReSharper restore UnusedAutoPropertyAccessor.Global
}