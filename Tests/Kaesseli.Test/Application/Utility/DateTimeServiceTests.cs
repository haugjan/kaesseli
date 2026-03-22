using FluentAssertions;
using Kaesseli.Application.Utility;
using Xunit;

namespace Kaesseli.Application.Test.Utility;

public class DateTimeServiceTests
{
    [Fact]
    public void ToDay_ReturnsCurrentDay()
    {
        //Arrange
        var dateTimeService = new DateTimeService();

        //Act
        WaitShortyIfItsJustBeforeMidnight();
        var currentDay = dateTimeService.ToDay;
        var expected = DateOnly.FromDateTime(DateTime.Today);

        //Assert
        currentDay.Should().Be(expected);
    }

    [Fact]
    public void TimeOfDay_ReturnsCurrentTime()
    {
        //Arrange
        var dateTimeService = new DateTimeService();
        var tolerance = TimeSpan.FromSeconds(value: 1);

        // Act
        var actualTime = dateTimeService.TimeOfDay;
        var expectedTime = TimeOnly.FromDateTime(DateTime.Now);

        // Assert
        var difference = (expectedTime - actualTime).Duration();

        difference.Should().BeLessOrEqualTo(tolerance);
    }

    [Fact]
    public void Now_ReturnsCurrentTime()
    {
        //Arrange
        var dateTimeService = new DateTimeService();
        var tolerance = TimeSpan.FromSeconds(value: 1);

        // Act
        var actual = dateTimeService.Now;
        var expected = DateTime.Now;

        // Assert
        var difference = (expected - actual).Duration();

        difference.Should().BeLessOrEqualTo(tolerance);
    }

    [Fact]
    public void UtcNow_ReturnsCurrentTime()
    {
        //Arrange
        var dateTimeService = new DateTimeService();
        var tolerance = TimeSpan.FromSeconds(value: 1);

        // Act
        var actual = dateTimeService.UtcNow;
        var expected = DateTimeOffset.UtcNow;

        // Assert
        var difference = (expected - actual).Duration();

        difference.Should().BeLessOrEqualTo(tolerance);
    }

    private static void WaitShortyIfItsJustBeforeMidnight()
    {
        var now = DateTime.Now;
        if (now.AddSeconds(value: 10).Day != now.Day) Thread.Sleep(timeout: TimeSpan.FromSeconds(value: 15));
    }
}
