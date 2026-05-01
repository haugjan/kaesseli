namespace Kaesseli.Features.Automation;

public class AutomationAccountShortNameNotFoundException(string shortName)
    : Exception(
        $"Automation references account short name '{shortName}', but no such account exists."
    );
