namespace Iris.Core.Tests;

/// <summary>Video tools that answer from data and write down what they were asked to do.</summary>
internal sealed class FakeTools : IVideoTools
{
    public Dictionary<string, MediaInfo> Infos { get; } = new();
    public DecodeLog Log { get; set; } = new([], [], 0);
    public Func<TimeSpan, TimeSpan, IReadOnlyList<Frame>> Window { get; set; } = (_, _) => [];
    public Func<TimeSpan, TimeSpan?> KeyFrame { get; set; } = _ => null;
    public long Duplicated { get; set; }

    public List<(TimeSpan From, TimeSpan Length, string Output)> Copies { get; } = new();
    public List<(IReadOnlyList<string> Parts, string Output)> Joins { get; } = new();
    public List<(string Output, string Encoder, string FrameRate)> Encodes { get; } = new();

    public Dictionary<string, VideoDetails> AllDetails { get; } = new();
    public Dictionary<string, IReadOnlyList<TimeSpan>> Keys { get; } = new();
    public Dictionary<string, (long Frames, long Distinct)> Samples { get; } = new();
    public List<(string Path, TimeSpan From, TimeSpan Length)> SampleCalls { get; } = new();
    public List<(string Path, TimeSpan At, string Output)> Stills { get; } = new();

    public MediaInfo Probe(string path) => Infos[path];

    public VideoDetails Details(string path) => AllDetails[path];

    public IReadOnlyList<TimeSpan> KeyFrames(string path, CancellationToken cancel) => Keys[path];

    public (long Frames, long Distinct) Repeats(string path, TimeSpan from, TimeSpan length, CancellationToken cancel)
    {
        SampleCalls.Add((path, from, length));
        return Samples[path];
    }

    public void Still(string path, TimeSpan at, string output, CancellationToken cancel) => Stills.Add((path, at, output));

    public DecodeLog Decode(string path, TimeSpan duration, IProgress<double>? progress, CancellationToken cancel) => Log;

    public IReadOnlyList<Frame> Frames(string path, TimeSpan from, TimeSpan to, CancellationToken cancel) => Window(from, to);

    public TimeSpan? KeyFrameFrom(string path, TimeSpan at, CancellationToken cancel) => KeyFrame(at);

    public void Copy(string path, TimeSpan from, TimeSpan length, string output, CancellationToken cancel) =>
        Copies.Add((from, length, output));

    public void Join(IReadOnlyList<string> parts, string output, CancellationToken cancel) => Joins.Add((parts, output));

    public long EncodeConstantRate(string path, string output, MediaInfo info, string encoder, IProgress<double>? progress, CancellationToken cancel)
    {
        Encodes.Add((output, encoder, info.FrameRate));
        return Duplicated;
    }
}

internal sealed class FakeDisk : IDisk
{
    public HashSet<string> Files { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Created { get; } = new();
    public List<string> Deleted { get; } = new();
    public List<(string From, string To)> Moves { get; } = new();

    public bool Exists(string path) => Files.Contains(path);

    public void CreateFolder(string path) => Created.Add(path);

    public void DeleteFolder(string path) => Deleted.Add(path);

    public void Move(string from, string to)
    {
        if (Files.Contains(to)) throw new IOException("exists: " + to);
        Moves.Add((from, to));
        Files.Add(to);
    }
}

/// <summary>A program that prints the given lines and ends with the given code; the calls are written down.</summary>
internal sealed class FakeRunner : IProcessRunner
{
    public Func<string, IReadOnlyList<string>, IEnumerable<string>> Lines { get; set; } = (_, _) => [];
    public int ExitCode { get; set; }
    public List<(string Program, IReadOnlyList<string> Args)> Calls { get; } = new();

    public (int ExitCode, string Tail) Run(string program, IReadOnlyList<string> args, Action<string> onLine, CancellationToken cancel)
    {
        Calls.Add((program, args.ToList()));
        foreach (var line in Lines(program, args)) onLine(line);
        return (ExitCode, "the last lines");
    }
}

internal static class Video
{
    public static TimeSpan S(double seconds) => TimeSpan.FromSeconds(seconds);

    /// <summary>Frames every 1/fps from <paramref name="from"/> (inclusive) to <paramref name="to"/> (exclusive), a key frame every <paramref name="keyEvery"/> seconds.</summary>
    public static IEnumerable<Frame> Run(double from, double to, double fps = 25, double keyEvery = 2)
    {
        long first = (long)Math.Round(from * fps), last = (long)Math.Round(to * fps);
        long keyFrames = (long)Math.Round(keyEvery * fps);
        for (long n = first; n < last; n++)
            yield return new Frame(TimeSpan.FromSeconds(n / fps), n % keyFrames == 0);
    }
}
