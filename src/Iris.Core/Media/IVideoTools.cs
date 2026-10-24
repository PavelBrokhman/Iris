namespace Iris.Core;

/// <param name="FrameRate">As ffprobe gives it ("30000/1001"), for ffmpeg's -r.</param>
/// <param name="VideoCodec">The picture's codec as ffprobe names it: h264, hevc...</param>
public sealed record MediaInfo(string Path, TimeSpan Duration, string FrameRate, double Fps, string? VideoCodec);

/// <summary>A frame that came out of the decoder: its time, and whether it is a key frame.</summary>
public sealed record Frame(TimeSpan At, bool Key);

/// <summary>One complaint of the decoder about the data, and how many frames had come out before it.</summary>
public sealed record Complaint(string Text, int FramesBefore);

/// <summary>What decoding the whole picture gave: the time of every frame in order, the complaints, the exit code.</summary>
public sealed record DecodeLog(IReadOnlyList<TimeSpan> Frames, IReadOnlyList<Complaint> Complaints, int ExitCode);

/// <summary>What a file says about itself.</summary>
/// <param name="ReportedRate">The frame rate the stream claims (ffprobe's r_frame_rate).</param>
/// <param name="AverageRate">Frames divided by length (avg_frame_rate) - the real one when the two differ.</param>
/// <param name="BFrames">How many B-frames may stand between others; 0 - none.</param>
/// <param name="BitRate">Of the picture in bit/s; 0 - unknown.</param>
public sealed record VideoDetails(string Path, TimeSpan Duration, int Width, int Height, string? Codec, string? Profile,
    int BFrames, double ReportedRate, double AverageRate, long BitRate, long Size, string? AudioCodec)
{
    public long Frames => (long)Math.Round(Duration.TotalSeconds * AverageRate);
}

/// <summary>
/// Everything Iris needs a video tool for - for now ffmpeg on this computer, later the Gateway's commands
/// (docs/decisions/umbrella-direction.md). The recipes get data from it, never the lines of a program.
/// </summary>
public interface IVideoTools
{
    MediaInfo Probe(string path);

    VideoDetails Details(string path);

    /// <summary>The times of all key frames, from the packets - nothing is decoded.</summary>
    IReadOnlyList<TimeSpan> KeyFrames(string path, CancellationToken cancel);

    /// <summary>Decodes a piece and counts its frames, and those left when a frame that repeats the one before is dropped.</summary>
    (long Frames, long Distinct) Repeats(string path, TimeSpan from, TimeSpan length, CancellationToken cancel);

    /// <summary>Writes the frame at <paramref name="at"/> as a picture.</summary>
    void Still(string path, TimeSpan at, string output, CancellationToken cancel);

    /// <summary>Decodes the whole picture and writes nothing.</summary>
    /// <param name="progress">How far, from 0 to 1.</param>
    DecodeLog Decode(string path, TimeSpan duration, IProgress<double>? progress, CancellationToken cancel);

    /// <summary>Decodes the frames from <paramref name="from"/> to <paramref name="to"/>; the times stay those of the video.</summary>
    IReadOnlyList<Frame> Frames(string path, TimeSpan from, TimeSpan to, CancellationToken cancel);

    /// <summary>The first key frame at or after <paramref name="at"/>; null when there is none to the end.</summary>
    TimeSpan? KeyFrameFrom(string path, TimeSpan at, CancellationToken cancel);

    /// <summary>Copies a piece without encoding it again; <paramref name="from"/> is a key frame.</summary>
    void Copy(string path, TimeSpan from, TimeSpan length, string output, CancellationToken cancel);

    /// <summary>Joins the pieces one after another without encoding them again.</summary>
    void Join(IReadOnlyList<string> parts, string output, CancellationToken cancel);

    /// <summary>
    /// Encodes the picture again at a constant frame rate, the sound copied. Where nothing can be decoded the
    /// last frame is repeated. Returns how many frames were such copies.
    /// </summary>
    long EncodeConstantRate(string path, string output, MediaInfo info, string encoder, IProgress<double>? progress, CancellationToken cancel);
}
