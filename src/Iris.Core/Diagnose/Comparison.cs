using System.Globalization;

namespace Iris.Core;

/// <summary>One of the two files compared, with what a piece of it showed about repeated frames.</summary>
public sealed record CompareSide(VideoDetails Details, long SampleFrames, long SampleDistinct)
{
    /// <summary>Frames per second that are not a repeat of the frame before; null when no frame of the piece decoded.</summary>
    public double? DistinctRate => SampleFrames > 0 ? Details.AverageRate * SampleDistinct / SampleFrames : null;
}

/// <summary>
/// Two files side by side: the same length or not, and which is better on the numbers - the picture
/// size and the frames that are not repeats. Which picture looks better is Pavel's call: the stills are for him.
/// Case: Nevestka4.mp4 against vid_720p (7).mp4 - the same film, the new one with 30 real frames/s.
/// </summary>
public sealed class Comparison
{
    /// <summary>Two files this close in length may be the same film.</summary>
    private static readonly TimeSpan SameLengthTolerance = TimeSpan.FromSeconds(2);

    /// <summary>Picture sizes or real frame rates closer than this share are alike.</summary>
    private const double Margin = 0.05;

    private static readonly CultureInfo Text = CultureInfo.InvariantCulture;

    public Comparison(CompareSide a, CompareSide b)
    {
        A = a;
        B = b;
        var (da, db) = (a.Details, b.Details);
        SameLength = (da.Duration - db.Duration).Duration() <= SameLengthTolerance;

        var lines = new List<string>
        {
            Row("Length", Clock.Short(da.Duration), Clock.Short(db.Duration)),
            Row("Picture", $"{da.Width}×{da.Height}", $"{db.Width}×{db.Height}"),
            Row("Frames/s", Rate(da.AverageRate), Rate(db.AverageRate)),
            Row("Real frames/s (repeats dropped, in a piece)", Distinct(a), Distinct(b)),
            Row("Bit rate, kbit/s", (da.BitRate / 1000).ToString(Text), (db.BitRate / 1000).ToString(Text)),
            Row("Codec", $"{da.Codec} {da.Profile}", $"{db.Codec} {db.Profile}"),
            Row("File, MB", (da.Size / 1_000_000).ToString(Text), (db.Size / 1_000_000).ToString(Text)),
            SameLength
                ? "The same length - perhaps the same film; the pictures themselves are not compared."
                : $"The lengths differ by {Clock.Short((da.Duration - db.Duration).Duration())}: not the same cut, or not the same film.",
        };

        var reasons = new List<(int Side, string Why)>();
        if (Wins(da.Width * (double)da.Height, db.Width * (double)db.Height) is { } bigger)
            reasons.Add((bigger, "a bigger picture"));
        if (a.DistinctRate is { } ra && b.DistinctRate is { } rb && Wins(ra, rb) is { } smoother)
            reasons.Add((smoother, "more real frames, fewer repeats"));
        Better = reasons.Count > 0 && reasons.All(r => r.Side == reasons[0].Side) ? reasons[0].Side : null;

        lines.Add(Better is { } side
            ? $"On the numbers the {(side == 0 ? "first" : "second")} is better: {string.Join(", ", reasons.Select(r => r.Why))}. How the picture looks - see the stills."
            : reasons.Count > 0
                ? "No clear winner: " + string.Join("; ", reasons.Select(r => $"the {(r.Side == 0 ? "first" : "second")} has {r.Why}")) + ". See the stills."
                : "Alike on the numbers. See the stills.");
        lines.Add("Damage is not looked for here - iris check each file for that.");
        Lines = lines;
    }

    public CompareSide A { get; }

    public CompareSide B { get; }

    public bool SameLength { get; }

    /// <summary>0 - the first, 1 - the second, null - no clear winner on the numbers.</summary>
    public int? Better { get; }

    /// <summary>For a person.</summary>
    public IReadOnlyList<string> Lines { get; }

    /// <summary>The pictures written for Pavel, in pairs from the same moments.</summary>
    public IReadOnlyList<string> Stills { get; init; } = [];

    private static int? Wins(double a, double b) =>
        Math.Abs(a - b) <= Math.Max(a, b) * Margin ? null : a > b ? 0 : 1;

    private static string Rate(double r) => r.ToString("0.##", Text);

    private static string Distinct(CompareSide side) => side.DistinctRate is { } r ? r.ToString("0.0", Text) : "none decoded";

    private static string Row(string name, string a, string b) => $"{name}: {a} | {b}";
}

/// <summary>Asks the video tools about two files and compares them; the stills only when a folder is given.</summary>
public sealed class Compare
{
    /// <summary>The piece decoded to count repeated frames: from the middle of the shorter file.</summary>
    private static readonly TimeSpan SampleLength = TimeSpan.FromSeconds(60);

    /// <summary>Stills are taken at these shares of the shorter length.</summary>
    private static readonly double[] StillMoments = [0.25, 0.5, 0.75];

    private readonly IVideoTools _tools;
    private readonly IDisk _disk;

    public Compare(IVideoTools tools, IDisk disk)
    {
        _tools = tools;
        _disk = disk;
    }

    /// <param name="stillsFolder">Where to write the stills (1-a.png, 1-b.png...); null - none. An existing still is not overwritten.</param>
    public Comparison Run(string a, string b, string? stillsFolder, CancellationToken cancel)
    {
        var (da, db) = (_tools.Details(a), _tools.Details(b));
        var shorter = da.Duration < db.Duration ? da.Duration : db.Duration;
        var length = shorter < SampleLength ? shorter : SampleLength;
        var from = shorter / 2 - length / 2;

        var (fa, ka) = _tools.Repeats(a, from, length, cancel);
        var (fb, kb) = _tools.Repeats(b, from, length, cancel);

        var stills = new List<string>();
        if (stillsFolder is not null)
        {
            _disk.CreateFolder(stillsFolder);
            for (int i = 0; i < StillMoments.Length; i++)
            {
                var at = shorter * StillMoments[i];
                foreach (var (path, side) in new[] { (a, "a"), (b, "b") })
                {
                    string still = Path.Combine(stillsFolder, $"{i + 1}-{side}.png");
                    _tools.Still(path, at, still, cancel);
                    stills.Add(still);
                }
            }
        }
        return new Comparison(new CompareSide(da, fa, ka), new CompareSide(db, fb, kb)) { Stills = stills };
    }
}
