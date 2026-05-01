namespace Kaesseli.Features.Accounts;

public class InvalidAccountShortNameException(string? shortName)
    : Exception(
        $"Account short name '{shortName}' is invalid. "
            + "Expected: 2-20 lowercase letters, digits or hyphens."
    );
