namespace Kaesseli.Application.Common;

public interface IDateTimeService
{
    // ReSharper disable UnusedMember.Global
    // ReSharper disable UnusedMemberInSuper.Global
    DateTime Now { get; }
    DateTimeOffset UtcNow { get; }
    DateOnly ToDay { get; }
    TimeOnly TimeOfDay { get; }
    // ReSharper restore UnusedMember.Global
    // ReSharper restore UnusedMemberInSuper.Global
}