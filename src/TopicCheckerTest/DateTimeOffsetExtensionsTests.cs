// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeOffsetExtensionsTests.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A test class to test the <see cref="DateTimeOffsetExtensions" /> class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TopicCheckerTest;

/// <summary>
///     A test class to test the <see cref="DateTimeOffsetExtensions" /> class. The end of the current month is the
///     expiration date of the data limit cache, so a wrong value locks a throttled user for the wrong time span.
/// </summary>
[TestClass]
public class DateTimeOffsetExtensionsTests
{
    /// <summary>
    ///     Checks whether the last second of the month is returned.
    /// </summary>
    [TestMethod]
    public void EndOfCurrentMonthReturnsTheLastSecondOfTheMonth()
    {
        var date = new DateTimeOffset(2026, 8, 14, 12, 30, 15, TimeSpan.FromHours(2));

        var result = date.EndOfCurrentMonth();

        Assert.AreEqual(2026, result.Year);
        Assert.AreEqual(8, result.Month);
        Assert.AreEqual(31, result.Day);
        Assert.AreEqual(23, result.Hour);
        Assert.AreEqual(59, result.Minute);
        Assert.AreEqual(59, result.Second);
    }

    /// <summary>
    ///     Checks whether the additional day of a leap year is taken into account.
    /// </summary>
    [TestMethod]
    public void EndOfCurrentMonthRespectsTheLeapYear()
    {
        var leapYear = new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.FromHours(1));
        var normalYear = new DateTimeOffset(2026, 2, 1, 0, 0, 0, TimeSpan.FromHours(1));

        Assert.AreEqual(29, leapYear.EndOfCurrentMonth().Day);
        Assert.AreEqual(28, normalYear.EndOfCurrentMonth().Day);
    }

    /// <summary>
    ///     Checks whether the offset of the local time zone is used. The offset used to be hard coded to one or two
    ///     hours, which is only correct for Central Europe and silently wrong everywhere else.
    /// </summary>
    [TestMethod]
    public void EndOfCurrentMonthUsesTheLocalTimeZoneOffset()
    {
        var date = new DateTimeOffset(2026, 8, 14, 12, 30, 15, TimeSpan.FromHours(2));
        var expectedOffset = TimeZoneInfo.Local.GetUtcOffset(new DateTime(2026, 8, 31, 23, 59, 59));

        var result = date.EndOfCurrentMonth();

        Assert.AreEqual(expectedOffset, result.Offset);
    }

    /// <summary>
    ///     Checks whether the offset follows the daylight saving time of the local time zone. In a time zone without
    ///     daylight saving time both offsets are the same, which the test allows on purpose.
    /// </summary>
    [TestMethod]
    public void EndOfCurrentMonthFollowsTheDaylightSavingTime()
    {
        var winter = new DateTimeOffset(2026, 1, 15, 12, 0, 0, TimeSpan.FromHours(1));
        var summer = new DateTimeOffset(2026, 7, 15, 12, 0, 0, TimeSpan.FromHours(2));

        Assert.AreEqual(TimeZoneInfo.Local.GetUtcOffset(new DateTime(2026, 1, 31, 23, 59, 59)), winter.EndOfCurrentMonth().Offset);
        Assert.AreEqual(TimeZoneInfo.Local.GetUtcOffset(new DateTime(2026, 7, 31, 23, 59, 59)), summer.EndOfCurrentMonth().Offset);
    }
}
