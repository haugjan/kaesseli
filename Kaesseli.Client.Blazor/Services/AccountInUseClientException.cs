namespace Kaesseli.Client.Blazor.Services;

public class AccountInUseClientException()
    : Exception("Konto kann nicht gelöscht werden, weil noch Buchungen darauf vorhanden sind.");
