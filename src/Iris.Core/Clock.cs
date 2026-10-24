namespace Iris.Core;

/// <summary>Times for a person.</summary>
public static class Clock
{
    /// <summary>1:02:03 or 02:03.</summary>
    public static string Short(TimeSpan t) => t.ToString(t.TotalHours >= 1 ? @"h\:mm\:ss" : @"mm\:ss");

    /// <summary>1:02:03.45 or 02:03.45 - where a frame matters.</summary>
    public static string Exact(TimeSpan t) => t.ToString(t.TotalHours >= 1 ? @"h\:mm\:ss\.ff" : @"mm\:ss\.ff");
}
