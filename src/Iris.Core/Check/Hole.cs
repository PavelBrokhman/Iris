namespace Iris.Core;

/// <summary>When a gap between two frames is a hole - nothing could be decoded there - and not an ordinary frame gap.</summary>
internal static class Hole
{
    private static readonly TimeSpan MinLength = TimeSpan.FromSeconds(0.5);

    /// <summary>A hole is also at least this many frames long.</summary>
    private const int MinFrames = 5;

    public static TimeSpan Min(double fps) =>
        TimeSpan.FromSeconds(Math.Max(MinLength.TotalSeconds, MinFrames / Math.Max(1, fps)));
}
