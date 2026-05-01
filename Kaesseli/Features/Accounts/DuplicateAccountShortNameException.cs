namespace Kaesseli.Features.Accounts;

public class DuplicateAccountShortNameException(string shortName)
    : Exception($"An account with short name '{shortName}' already exists.");
