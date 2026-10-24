namespace Iris.Core;

/// <summary>Looks for damaged places in a video. The video is only read.</summary>
public interface IVideoCheck
{
    /// <param name="progress">How far the check is, from 0 to 1.</param>
    CheckReport Check(string path, IProgress<double>? progress, CancellationToken cancel);
}

/// <summary>One complaint of the decoder, with the place in the video it was reading at that moment.</summary>
public sealed record DamageMessage(TimeSpan At, string Text);

/// <summary>Complaints that lie close together, taken as one damaged place.</summary>
public sealed record DamagedPlace(TimeSpan From, TimeSpan To, IReadOnlyList<DamageMessage> Messages);

/// <param name="Checked">How far the decoder got. Less than <paramref name="Duration"/> means it gave up early.</param>
/// <param name="Codec">The picture's codec as ffprobe names it: h264, hevc...</param>
public sealed record CheckReport(string Path, TimeSpan Duration, TimeSpan Checked, int ExitCode,
    IReadOnlyList<DamagedPlace> Places, string? Codec = null)
{
    /// <summary>The decoder may stop a second or so before the length given in the file.</summary>
    public static readonly TimeSpan EndTolerance = TimeSpan.FromSeconds(2);

    public bool Damaged => Places.Count > 0 || ExitCode != 0 || !ReadToTheEnd;

    public bool ReadToTheEnd => Duration - Checked < EndTolerance;
}
