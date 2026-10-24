using System.Globalization;

namespace Iris.Core;

/// <summary>A fact about a file, and what it means for players and tools; null when it means nothing in particular.</summary>
public sealed record Finding(string Fact, string? Meaning);

/// <summary>
/// What a file says about itself, told with what each thing means - the meanings come from what was
/// seen with Pavel's videos (docs/knowledge.md), and say "not checked" where nothing was seen.
/// </summary>
public sealed class FileDiagnosis
{
    /// <summary>Explorer showed a picture as the thumbnail with a key frame every 2 s, a blank one with 8-10 s (knowledge.md, 2026-10-12).</summary>
    private const double ThumbnailShown = 2, ThumbnailBlank = 8;

    /// <summary>"Every 2 s" is often every 60 frames: 2.002 s at 29.97 frames/s.</summary>
    private const double IntervalTolerance = 0.05;

    /// <summary>Reported and average rates further apart than this share make a variable frame rate.</summary>
    private const double VariableShare = 0.01;

    /// <summary>
    /// Lada shows Remaining and Speed after reported rate × this many frames (lada/cli/utils.py:183); a file with
    /// fewer frames than that never gets them (vfr-frame-rate-90000.mp4, D:\Projects\lada\samples\README.md).
    /// </summary>
    private const double LadaEstimateSeconds = 15;

    /// <summary>A key frame gap this many times the usual one is not how the file was made - more likely damage.</summary>
    private const double UnusualGap = 10;

    private static readonly CultureInfo Text = CultureInfo.InvariantCulture;

    public FileDiagnosis(VideoDetails details, IReadOnlyList<TimeSpan> keyFrames)
    {
        Details = details;
        var findings = new List<Finding> { new(Describe(details), null) };
        findings.Add(KeyFrames(keyFrames));
        if (details.BFrames > 0) findings.Add(BFrames(details));
        if (Variable(details) is { } rate) findings.Add(rate);
        Findings = findings;
    }

    public VideoDetails Details { get; }

    public IReadOnlyList<Finding> Findings { get; }

    private static string Describe(VideoDetails d)
    {
        string bitRate = d.BitRate > 0 ? $"{d.BitRate / 1000} kbit/s" : "bit rate unknown";
        string sound = d.AudioCodec is { } a ? "sound " + a : "no sound";
        return string.Format(Text, "{0} {1}, {2}×{3}, {4:0.##} frames/s, {5}, {6}, {7} frames, {8}",
            d.Codec, d.Profile, d.Width, d.Height, d.AverageRate, bitRate, Clock.Short(d.Duration), d.Frames, sound);
    }

    private static Finding KeyFrames(IReadOnlyList<TimeSpan> keys)
    {
        if (keys.Count < 2)
            return new Finding(keys.Count == 0 ? "No key frame found" : "Only one key frame",
                "A player has to decode from the start to show any other place; seeking is very slow.");

        var gaps = keys.Zip(keys.Skip(1), (a, b) => (b - a).TotalSeconds).OrderBy(g => g).ToList();
        double typical = gaps[gaps.Count / 2], longest = gaps[^1];
        string fact = string.Format(Text, "A key frame every {0:0.0} s (the longest gap {1:0.0} s)", typical, longest);
        string meaning = typical >= ThumbnailBlank
            ? string.Format(Text, "Explorer likely shows a blank thumbnail (seen at 8-10 s); a player seeking here decodes up to {0:0} s of frames first, so seeking is slow.", longest)
            : typical <= ThumbnailShown + IntervalTolerance
                ? "Explorer shows a picture as its thumbnail (seen at 2 s); seeking is quick."
                : "Thumbnails not checked at this interval (2 s gave a picture, 8-10 s a blank one).";
        if (longest >= typical * UnusualGap)
            meaning += string.Format(Text, " A gap of {0:0} s is far longer than the others - perhaps damage; iris check tells.", longest);
        return new Finding(fact, meaning);
    }

    private static Finding BFrames(VideoDetails d) => d.Codec == "hevc"
        ? new Finding($"HEVC with B-frames (up to {d.BFrames} in a row)",
            "VLC may stutter at 4x and on ← / →, as with Lada's results - not checked whether its hardware decoding helps.")
        : new Finding($"B-frames (up to {d.BFrames} in a row)", null);

    private static Finding? Variable(VideoDetails d)
    {
        if (d.AverageRate <= 0 || Math.Abs(d.ReportedRate - d.AverageRate) / d.AverageRate <= VariableShare) return null;
        string fact = string.Format(Text, "Variable frame rate: reports {0:0.##} frames/s, really {1:0.##}", d.ReportedRate, d.AverageRate);
        string meaning = "An encode at a constant rate (a freeze repair) adds repeated frames here and there.";
        if (d.ReportedRate * LadaEstimateSeconds > d.Frames)
            meaning = "Lada's Remaining and Speed stay \"?\" the whole job: it waits for more frames than the file has. " + meaning;
        return new Finding(fact, meaning);
    }
}

/// <summary>Asks the video tools about a file and tells what it means.</summary>
public sealed class Diagnose
{
    private readonly IVideoTools _tools;

    public Diagnose(IVideoTools tools)
    {
        _tools = tools;
    }

    public FileDiagnosis Run(string path, CancellationToken cancel) =>
        new(_tools.Details(path), _tools.KeyFrames(path, cancel));
}
