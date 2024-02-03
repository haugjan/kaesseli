namespace Kaesseli.Application.Common;

public interface IDateTimeService
{
    DateTime Now { get; }
    DateTimeOffset UtcNow { get; }
    DateOnly ToDay { get; }
    TimeOnly TimeOfDay { get; }
}