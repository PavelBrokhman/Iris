namespace Iris.Core;

/// <summary>
/// Decodes the whole picture and writes nothing. Every complaint of the decoder is put at the last frame
/// that came out before it. While nothing can be decoded no frame comes out, so a damaged place starts at
/// the last good frame and ends at the first frame after the last complaint - both to a frame. No frame
/// after it: the damage goes on to the last frame read.
/// The decoder may also go silent: a few complaints, a few more frames, then no frame for minutes and no
/// word about it. So a hole in the frame times is a damaged place too, joined to the complaints next to it.
/// </summary>
public sealed class DecodeCheck : IVideoCheck
{
    /// <summary>Complaints further apart than this are separate damaged places.</summary>
    private static readonly TimeSpan Gap = TimeSpan.FromSeconds(3);

    private readonly IVideoTools _tools;

    public DecodeCheck(IVideoTools tools)
    {
        _tools = tools;
    }

    public CheckReport Check(string path, IProgress<double>? progress, CancellationToken cancel)
    {
        var info = _tools.Probe(path);
        var log = _tools.Decode(path, info.Duration, progress, cancel);
        var read = log.Frames.Count > 0 ? log.Frames[^1] : TimeSpan.Zero;
        var places = Join(Group(log).Concat(Holes(log.Frames, Hole.Min(info.Fps))));
        return new CheckReport(path, info.Duration, read, log.ExitCode, places, info.VideoCodec);
    }

    private static IEnumerable<DamagedPlace> Holes(IReadOnlyList<TimeSpan> frames, TimeSpan min)
    {
        for (int i = 1; i < frames.Count; i++)
        {
            var gap = frames[i] - frames[i - 1];
            if (gap > min)
                yield return new DamagedPlace(frames[i - 1], frames[i], [new DamageMessage(frames[i - 1], $"No frame for {Clock.Exact(gap)}")]);
        }
    }

    /// <summary>Places that overlap or lie closer than <see cref="Gap"/> become one.</summary>
    private static List<DamagedPlace> Join(IEnumerable<DamagedPlace> places)
    {
        var joined = new List<DamagedPlace>();
        foreach (var p in places.OrderBy(p => p.From))
        {
            if (joined.Count > 0 && p.From - joined[^1].To <= Gap)
            {
                var last = joined[^1];
                joined[^1] = new DamagedPlace(last.From, p.To > last.To ? p.To : last.To, last.Messages.Concat(p.Messages).ToList());
            }
            else
                joined.Add(p);
        }
        return joined;
    }

    private static List<DamagedPlace> Group(DecodeLog log)
    {
        var frames = log.Frames;
        var last = frames.Count > 0 ? frames[^1] : TimeSpan.Zero;
        var places = new List<DamagedPlace>();
        var current = new List<(DamageMessage Message, int Frames)>();

        void Close()
        {
            // the first frame that came out after the last complaint: decoding went on from there
            int next = current[^1].Frames;
            var to = next < frames.Count ? frames[next] : last;
            places.Add(new DamagedPlace(current[0].Message.At, to, current.Select(c => c.Message).ToList()));
            current = new();
        }

        foreach (var c in log.Complaints)
        {
            var at = c.FramesBefore > 0 ? frames[c.FramesBefore - 1] : TimeSpan.Zero;
            if (current.Count > 0 && at - current[^1].Message.At > Gap) Close();
            current.Add((new DamageMessage(at, c.Text), c.FramesBefore));
        }
        if (current.Count > 0) Close();
        return places;
    }
}
