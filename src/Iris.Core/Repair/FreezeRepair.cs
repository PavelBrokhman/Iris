namespace Iris.Core;

/// <summary>
/// Encodes the whole video again at a constant frame rate. Where nothing can be decoded there are
/// no frames, and ffmpeg fills such a hole with copies of the last frame it had - the damaged piece
/// becomes a still picture, the length stays. The sound is copied as is: damaged in the same place,
/// it stays damaged. The damaged place does not have to be found first.
/// </summary>
public sealed class FreezeRepair : IVideoRepair
{
    public const string RepairName = "freeze";

    /// <summary>The encoders it can use: on the processor, on an NVIDIA card (several times faster).</summary>
    public const string Processor = "libx264", Nvidia = "h264_nvenc";

    private readonly IVideoTools _tools;
    private readonly IDisk _disk;
    private readonly string _work;
    private readonly string _encoder;

    /// <param name="workFolder">Where the copy is written before it is moved to its place; deleted at the end.</param>
    public FreezeRepair(IVideoTools tools, IDisk disk, string workFolder, string encoder = Processor)
    {
        _tools = tools;
        _disk = disk;
        _work = workFolder;
        _encoder = encoder;
    }

    public string Name => RepairName;

    public string ResultWord => "frozen";

    public RepairResult Repair(string path, string outputPath, IProgress<double>? progress, CancellationToken cancel, CheckReport? known = null)
    {
        if (_disk.Exists(outputPath)) throw new IOException($"{outputPath} exists already; nothing was written.");

        var info = _tools.Probe(path);
        string encoded = Path.Combine(_work, "frozen.mp4");
        long duplicated;
        try
        {
            _disk.CreateFolder(_work);
            duplicated = _tools.EncodeConstantRate(path, encoded, info, _encoder, progress, cancel);
            _disk.Move(encoded, outputPath);
        }
        finally
        {
            _disk.DeleteFolder(_work); // with what a failed or stopped freeze wrote
        }

        var still = TimeSpan.FromSeconds(duplicated / Math.Max(1, info.Fps));
        return new RepairResult(outputPath, true,
            $"Encoded again; {duplicated} frame(s) were copies of the frame before them (about {Clock.Exact(still)} in all). " +
            "A video with a variable frame rate gets some such copies everywhere, not only at the damage.");
    }
}
