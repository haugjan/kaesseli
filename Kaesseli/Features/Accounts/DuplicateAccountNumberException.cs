namespace Kaesseli.Features.Accounts;

public class DuplicateAccountNumberException(string number)
    : Exception($"An account with number '{number}' already exists.");
