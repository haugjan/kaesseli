using Kaesseli.Domain.Accounts;

namespace Kaesseli.Application.Accounts;

// ReSharper disable once ClassNeverInstantiated.Global
public class AddAccountCommand
{
    public required string Name { get; init; }
    public required AccountType Type { get; init; }
    public required string Icon { get; init; }
    public required string IconColor { get; init; }
}