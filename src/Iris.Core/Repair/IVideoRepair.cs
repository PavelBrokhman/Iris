namespace Iris.Core;

/// <summary>Makes a copy of a damaged video that programs like Lada can read to the end. The video itself is only read.</summary>
public interface IVideoRepair
{
    /// <summary>The name to choose it by: "cut", "freeze".</summary>
    string Name { get; }

    /// <summary>Goes into the name of the result: video.cut.mp4.</summary>
    string ResultWord { get; }

    /// <param name="progress">How far the repair is, from 0 to 1.</param>
    /// <param name="known">What a check of this video found already; null - the repair checks it itself if it needs to.</param>
    RepairResult Repair(string path, string outputPath, IProgress<double>? progress, CancellationToken cancel, CheckReport? known = null);
}

/// <param name="Changed">False when nothing was damaged and no file was written.</param>
/// <param name="Summary">What was done, for a person.</param>
public sealed record RepairResult(string OutputPath, bool Changed, string Summary);

public interface IRepairFactory
{
    /// <param name="videoEncoder">For a repair that encodes again (freeze): the ffmpeg encoder; null - the processor (x264).</param>
    IVideoRepair Create(string name, string? videoEncoder = null);
}
