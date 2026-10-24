namespace Iris.Core;

/// <summary>
/// Cuts the damaged places out without encoding anything again: the good pieces are copied and
/// joined. Each piece after a damaged place has to start at a key frame, so a little more than the
/// damage itself may go. At each join the picture and the sound jump.
/// Steps: the whole video is checked (<see cref="IVideoCheck"/>); around each damaged place the frames
/// are decoded once more to find the last good frame before it and the first frame after it; the
/// first key frame from there is where the next piece starts. The pieces and the joined video are
/// written in the work folder, the joined video is moved to its place when it is whole, the work
/// folder is deleted at the end.
/// </summary>
public sealed class CutRepair : IVideoRepair
{
    public const string RepairName = "cut";

    /// <summary>The frames around a damaged place are decoded from this long before it to this long after it.</summary>
    private static readonly TimeSpan LookBefore = TimeSpan.FromSeconds(10), LookAfter = TimeSpan.FromSeconds(30);

    /// <summary>A joined video may differ from the sum of its pieces by this much and still be whole.</summary>
    private static readonly TimeSpan LengthTolerance = TimeSpan.FromSeconds(2);

    /// <summary>The share of the progress the check takes; the rest is cutting and joining.</summary>
    private const double CheckShare = 0.8, EdgesDone = 0.85;

    private readonly IVideoCheck _check;
    private readonly IVideoTools _tools;
    private readonly IDisk _disk;
    private readonly string _work;

    /// <param name="workFolder">Where the pieces and the joined video are written; deleted at the end.</param>
    public CutRepair(IVideoCheck check, IVideoTools tools, IDisk disk, string workFolder)
    {
        _check = check;
        _tools = tools;
        _disk = disk;
        _work = workFolder;
    }

    public string Name => RepairName;

    public string ResultWord => "cut";

    public RepairResult Repair(string path, string outputPath, IProgress<double>? progress, CancellationToken cancel, CheckReport? known = null)
    {
        if (_disk.Exists(outputPath)) throw new IOException($"{outputPath} exists already; nothing was written.");

        var info = _tools.Probe(path);
        var duration = info.Duration;
        var frame = TimeSpan.FromSeconds(1 / Math.Max(1, info.Fps));

        // 1. Where is the damage (the long part), unless a check found it already
        var report = known ?? _check.Check(path, Part(progress, 0, CheckShare), cancel);
        var diagnosis = new Diagnosis(report);
        if (diagnosis.Kind == DamageKind.None)
            return new RepairResult(outputPath, false, "No damage found; nothing was written.");
        // damaged to the end: the place where it starts is the last one, nothing after it is kept
        var places = report.Places.Where(p => diagnosis.WholeUntil is not { } until || p.From < until).ToList();

        // 2. Exact edges of each damaged place
        var keep = new List<(TimeSpan From, TimeSpan To)>();
        var removed = new List<(TimeSpan From, TimeSpan To)>();
        var cursor = TimeSpan.Zero;
        var toTheEnd = diagnosis.WholeUntil is { } start
            ? new DamagedPlace(start, report.Checked, report.Places.LastOrDefault(p => p.From >= start)?.Messages ?? [])
            : null;
        if (toTheEnd is not null) places.Add(toTheEnd);
        foreach (var place in places)
        {
            cancel.ThrowIfCancellationRequested();
            if (place.From < cursor) continue; // already inside the piece cut out for the place before
            var (lastGood, firstAfter) = Edges(path, place, duration, frame, cancel);
            if (place == toTheEnd) firstAfter = null; // frames that come after it are not to be trusted
            else if (firstAfter is null)
                // the decoder looks on past the window to the first frame, so no frame means none to the end - yet the
                // check read the video on: cutting here would cut away all the rest without saying so
                throw new InvalidOperationException(
                    $"The end of the damaged place at about {Clock.Exact(place.From)} was not found: no frame comes " +
                    "after it, though the check read the video on. Nothing was written.");
            var key = firstAfter is { } after ? _tools.KeyFrameFrom(path, after, cancel) : null;

            var end = lastGood + frame;
            if (end > cursor) keep.Add((cursor, end));
            removed.Add((end, key ?? duration));
            cursor = key ?? duration;
        }
        if (cursor < duration - frame) keep.Add((cursor, duration));
        if (keep.Count == 0) throw new InvalidOperationException("Nothing whole is left to keep; nothing was written.");
        progress?.Report(EdgesDone);

        // 3. Copy the good pieces and join them
        string joined = Path.Combine(_work, "joined.mp4");
        try
        {
            _disk.CreateFolder(_work);
            var parts = new List<string>();
            for (int i = 0; i < keep.Count; i++)
            {
                string part = Path.Combine(_work, $"part{i}.mp4");
                _tools.Copy(path, keep[i].From, keep[i].To - keep[i].From, part, cancel);
                parts.Add(part);
            }
            _tools.Join(parts, joined, cancel);

            // ffmpeg can stop joining on a bad packet and still end with code 0
            var expected = keep.Aggregate(TimeSpan.Zero, (sum, k) => sum + (k.To - k.From));
            var written = _tools.Probe(joined).Duration;
            if ((expected - written).Duration() > LengthTolerance)
                throw new InvalidOperationException(
                    $"The joined video is {Clock.Exact(written)} long instead of {Clock.Exact(expected)}; the result was deleted.");
            _disk.Move(joined, outputPath);
        }
        finally
        {
            _disk.DeleteFolder(_work); // the pieces, and what a failed or stopped cut wrote
        }
        progress?.Report(1);

        var holes = toTheEnd is null ? removed : removed.Take(removed.Count - 1).ToList();
        var summary = new List<string>();
        if (holes.Count > 0)
        {
            var gone = holes.Aggregate(TimeSpan.Zero, (sum, r) => sum + (r.To - r.From));
            summary.Add($"Cut out {holes.Count} piece(s), {Clock.Exact(gone)} in all: " +
                        string.Join(", ", holes.Select(r => $"{Clock.Exact(r.From)} - {Clock.Exact(r.To)}")) + ".");
        }
        if (toTheEnd is not null)
            summary.Add($"Kept the whole start up to {Clock.Exact(removed[^1].From)}; from there the video was damaged to the end.");
        return new RepairResult(outputPath, true, string.Join(" ", summary));
    }

    /// <summary>
    /// Decodes a window around the place and looks for the hole in the frames: the last frame before
    /// it and the first frame after it (null when nothing comes after it).
    /// </summary>
    private (TimeSpan LastGood, TimeSpan? FirstAfter) Edges(string path, DamagedPlace place, TimeSpan duration, TimeSpan frame, CancellationToken cancel)
    {
        var from = place.From - LookBefore;
        if (from < TimeSpan.Zero) from = TimeSpan.Zero;
        var to = place.To + LookAfter;
        if (to > duration) to = duration;

        var frames = _tools.Frames(path, from, to, cancel).OrderBy(f => f.At).ToList();
        var times = frames.Select(f => f.At.TotalSeconds).ToList();
        var keys = frames.Where(f => f.Key).Select(f => f.At.TotalSeconds).ToList();

        var hole = Hole.Min(1 / frame.TotalSeconds).TotalSeconds;
        int first = -1, last = -1; // the frames after which the first hole starts / before which the last hole ends
        for (int i = 1; i < times.Count; i++)
        {
            if (times[i] - times[i - 1] <= hole) continue;
            if (first < 0) first = i - 1;
            last = i;
        }

        // no frames up to the end of the window: the hole is not closed in it
        bool open = times.Count > 0 && to.TotalSeconds - times[^1] > hole;
        if (open)
        {
            if (first < 0) first = times.Count - 1;
            last = -1;
        }

        if (first < 0)
        {
            // no hole: frames came out all the time (broken ones too) - fall back on what the check saw
            if (times.Count == 0) return (place.From, place.To < duration ? place.To : null);
            if (place.From.TotalSeconds <= times[0] + frame.TotalSeconds) return (TimeSpan.Zero, place.To); // damage at the very start
            return (place.From, place.To < duration ? place.To : null);
        }
        // The good piece ends before the last key frame before the hole, not at the last good frame: in the file
        // frames are stored out of showing order, and the packets just before the hole are already damaged.
        // Up to one group of frames (a few seconds) more goes, the join is clean.
        double end = keys.Where(k => k <= times[first]).DefaultIfEmpty(times[first]).Max();
        var lastGood = TimeSpan.FromSeconds(end) - frame; // the caller adds one frame
        TimeSpan? firstAfter = last < 0 ? null : TimeSpan.FromSeconds(times[last]);
        return (lastGood, firstAfter);
    }

    private static IProgress<double>? Part(IProgress<double>? progress, double from, double to) =>
        progress is null ? null : new Progress<double>(p => progress.Report(from + (to - from) * p));
}
