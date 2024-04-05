namespace Kaesseli.Application.Utility;

internal class EnvironmentService : IEnvironmentService
{
    public string CurrentUser => Environment.UserName;
}