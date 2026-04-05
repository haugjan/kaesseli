namespace Kaesseli.Infrastructure;

internal class EnvironmentService : IEnvironmentService
{
    public string CurrentUser => Environment.UserName;
}