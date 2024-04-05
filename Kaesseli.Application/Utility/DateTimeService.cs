namespace Kaesseli.Application.Utility;

internal class DateTimeService: IDateTimeService
{
    public DateTime Now => DateTime.Now;

    public DateTimeOffset UtcNow => DateTimeOffset.Now;

    public DateOnly ToDay => DateOnly.FromDateTime(Now);
    public TimeOnly TimeOfDay => TimeOnly.FromDateTime(Now);
}