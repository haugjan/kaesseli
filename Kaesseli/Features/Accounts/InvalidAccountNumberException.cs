namespace Kaesseli.Features.Accounts;

public class InvalidAccountNumberException(string? number)
    : Exception(
        $"Account number '{number}' is invalid. Expected format: 4 digits, first digit 1-4."
    );
