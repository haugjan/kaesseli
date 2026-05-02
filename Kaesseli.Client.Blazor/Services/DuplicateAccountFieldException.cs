namespace Kaesseli.Client.Blazor.Services;

public class DuplicateAccountFieldException(string field, string message) : Exception(message)
{
    public string Field { get; } = field;
}
