// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DateTimeExtensions.cs" company="Hämmer Electronics">
//   Copyright (c) All rights reserved.
// </copyright>
// <summary>
//   A class that contains extension method for the <see cref="DateTime" /> data type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace NetCoreMQTTExampleJsonConfigHashedPasswords;

/// <summary>
/// A class that contains extension method for the <see cref="DateTime"/> data type.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Gets the time zone offset of the local time zone.
    /// </summary>
    /// <param name="date">The date to get the time zone offset from.</param>
    /// <returns>The time zone offset of the local time zone</returns>
    public static TimeSpan GetTimeZoneOffset(this DateTime date)
    {
        return TimeZoneInfo.Local.IsDaylightSavingTime(date) ? TimeSpan.FromHours(2) : TimeSpan.FromHours(1);
    }
}
