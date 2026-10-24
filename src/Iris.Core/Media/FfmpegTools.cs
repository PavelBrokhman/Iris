using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Iris.Core;

/// <summary>
/// The video tools as ffmpeg and ffprobe on this computer. Moved from Mosaic Remover's
/// Diagnostic classes (docs/decisions/check-recipes.md); the ffmpeg arguments and their reasons are kept.
/// </summary>
public sealed class FfmpegTools : IVideoTools
{
    /// <summary>How often ffmpeg tells how far it is, in seconds - for the progress only.</summary>
    private const string ProgressPeriod = "0.5";

    /// <summary>A rate ffprobe does not know is taken as this, so that ffmpeg's -r gets something.</summary>
    private const string UnknownRate = "25";

    /// <summary>Seeking to a key frame goes this much after it, so that it cannot land on the key frame before it.</summary>
    private static readonly TimeSpan Hair = TimeSpan.FromMilliseconds(1);

    /// <summary>High quality, so the copy is hardly worse than the video: x264 at CRF 16; NVENC at the same look, constant quality 19.</summary>
    private const string ProcessorQuality = "16", ProcessorPreset = "veryfast";
    private const string NvidiaQuality = "19", NvidiaPreset = "p5";

    // "[h264 @ 000001aa0aa3e1c0] [error] Invalid NAL unit size" - the level is there because of "-v level+info"
    private static readonly Regex ComplaintLevel = new(@"\[(error|fatal|panic)\]\s*", RegexOptions.Compiled);
    private static readonly Regex Address = new(@" @ [0-9a-fA-F]+\]", RegexOptions.Compiled);

    // The output that writes nothing ("-f null") and its stream: they complain about frame times
    // ("non monotonically increasing dts to muxer") where the decoder is silent - not about the video.
    private static readonly Regex OutputSide = new(@"^\[(null|vost#\S*) @ ", RegexOptions.Compiled);
    private static readonly Regex ShowinfoTime = new(@"Parsed_showinfo.*\spts_time:\s*(-?[\d.]+)", RegexOptions.Compiled);

    private readonly IProcessRunner _runner;
    private readonly string? _folder;

    /// <param name="ffmpegFolder">Where ffmpeg.exe and ffprobe.exe are; null - on the PATH.</param>
    public FfmpegTools(IProcessRunner runner, string? ffmpegFolder)
    {
        _runner = runner;
        _folder = ffmpegFolder;
    }

    public MediaInfo Probe(string path)
    {
        var json = new StringBuilder();
        Run("ffprobe", new[] { "-v", "error", "-show_streams", "-show_format", "-of", "json", path }, line => json.AppendLine(line), CancellationToken.None);

        using var doc = JsonDocument.Parse(json.ToString());
        JsonElement? video = null;
        foreach (var s in doc.RootElement.GetProperty("streams").EnumerateArray())
            if (Text(s, "codec_type") == "video") { video = s; break; }
        if (video is not { } v) throw new InvalidOperationException("No video stream in " + path);

        string rate = Text(v, "avg_frame_rate") ?? "0/0";
        if (Rate(rate) <= 0) rate = Text(v, "r_frame_rate") ?? UnknownRate;
        if (Rate(rate) <= 0) rate = UnknownRate;

        double duration = Number(Text(v, "duration"));
        if (duration <= 0 && doc.RootElement.TryGetProperty("format", out var format)) duration = Number(Text(format, "duration"));

        return new MediaInfo(path, TimeSpan.FromSeconds(duration), rate, Rate(rate), Text(v, "codec_name"));
    }

    public VideoDetails Details(string path)
    {
        var json = new StringBuilder();
        Run("ffprobe", new[] { "-v", "error", "-show_streams", "-show_format", "-of", "json", path }, line => json.AppendLine(line), CancellationToken.None);

        using var doc = JsonDocument.Parse(json.ToString());
        JsonElement? video = null, audio = null;
        foreach (var s in doc.RootElement.GetProperty("streams").EnumerateArray())
        {
            string type = Text(s, "codec_type") ?? "";
            if (type == "video" && video is null) video = s;
            if (type == "audio" && audio is null) audio = s;
        }
        if (video is not { } v) throw new InvalidOperationException("No video stream in " + path);
        var format = doc.RootElement.TryGetProperty("format", out var f) ? f : (JsonElement?)null;

        double duration = Number(Text(v, "duration"));
        if (duration <= 0 && format is { } fd) duration = Number(Text(fd, "duration"));
        long bitRate = (long)Number(Text(v, "bit_rate"));
        if (bitRate <= 0 && format is { } fb) bitRate = (long)Number(Text(fb, "bit_rate"));

        return new VideoDetails(path, TimeSpan.FromSeconds(duration), Int(v, "width"), Int(v, "height"),
            Text(v, "codec_name"), Text(v, "profile"), Int(v, "has_b_frames"),
            Rate(Text(v, "r_frame_rate") ?? "0"), Rate(Text(v, "avg_frame_rate") ?? "0"),
            bitRate, format is { } fs ? (long)Number(Text(fs, "size")) : 0,
            audio is { } a ? Text(a, "codec_name") : null);
    }

    public IReadOnlyList<TimeSpan> KeyFrames(string path, CancellationToken cancel)
    {
        var keys = new List<TimeSpan>();
        Run("ffprobe", new[]
        {
            // only the packets are listed, nothing is decoded - quick even for hours
            "-v", "quiet", "-select_streams", "v:0", "-show_entries", "packet=pts_time,flags", "-of", "csv=p=0", path,
        }, line =>
        {
            var cells = line.Trim().Split(',');
            if (cells.Length >= 2 && cells[1].Contains('K') && TryTime(cells[0], out var t)) keys.Add(t);
        }, cancel);
        keys.Sort();
        return keys;
    }

    public (long Frames, long Distinct) Repeats(string path, TimeSpan from, TimeSpan length, CancellationToken cancel)
    {
        long frames = 0, kept = 0;
        Run("ffmpeg", new[]
        {
            "-hide_banner", "-nostats", "-v", "info", "-progress", "pipe:1",
            "-ss", Seconds(from), "-t", Seconds(length), "-i", path, "-map", "0:v:0",
            // showinfo sees every frame, mpdecimate then drops a frame that repeats the one before; "frame=" counts what is left
            "-vf", "showinfo,mpdecimate",
            "-fps_mode", "passthrough", // else the null output would fill the dropped frames in again
            "-max_error_rate", "1", // the piece may be all damage: by default ffmpeg gives up above 2/3 bad frames
            "-f", "null", "-",
        }, line =>
        {
            line = line.Trim();
            // "frame=46" of the progress; ffmpeg's summary "frame=   46 fps=..." is not a number and is skipped
            if (line.StartsWith("frame=", StringComparison.Ordinal))
            {
                if (long.TryParse(line["frame=".Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out long n)) kept = n;
            }
            else if (ShowinfoTime.IsMatch(line))
                frames++;
        }, cancel);
        return (frames, kept);
    }

    public void Still(string path, TimeSpan at, string output, CancellationToken cancel) =>
        Run("ffmpeg", new[]
        {
            "-hide_banner", "-v", "error", "-n", // -n: an existing picture is not overwritten, ffmpeg fails instead
            "-ss", Seconds(at), "-i", path, "-frames:v", "1", output,
        }, _ => { }, cancel);

    public DecodeLog Decode(string path, TimeSpan duration, IProgress<double>? progress, CancellationToken cancel)
    {
        var frames = new List<TimeSpan>();
        var complaints = new List<Complaint>();

        void OnLine(string line)
        {
            line = line.Trim();
            if (Progress(line, duration, progress)) return;
            var frame = ShowinfoTime.Match(line);
            if (frame.Success)
            {
                if (TryTime(frame.Groups[1].Value, out var t)) frames.Add(t);
                return;
            }
            var level = ComplaintLevel.Match(line);
            if (!level.Success || line.Contains("Last message repeated", StringComparison.Ordinal) || OutputSide.IsMatch(line)) return;
            // "[h264 @ 000001aa0aa3e1c0] [error] ..." -> "[h264] ...": the same complaint reads the same
            complaints.Add(new Complaint(Address.Replace(line.Remove(level.Index, level.Length), "]"), frames.Count));
        }

        var (exitCode, _) = _runner.Run(Program("ffmpeg"), new[]
        {
            // info: for showinfo's lines; the level in each line tells the complaints from them
            "-hide_banner", "-nostats", "-v", "level+info",
            "-progress", "pipe:1", "-stats_period", ProgressPeriod,
            // only the picture: decoding damaged sound makes ffmpeg 8 stop halfway (exit code -22)
            "-i", path, "-map", "0:v:0",
            "-vf", "showinfo=checksum=0", // the frame times; no sums of the picture, they cost time
            "-max_error_rate", "1", // a short video can be damaged more than 2/3, then ffmpeg would give up
            "-f", "null", "-",
        }, OnLine, cancel);
        return new DecodeLog(frames, complaints, exitCode);
    }

    public IReadOnlyList<Frame> Frames(string path, TimeSpan from, TimeSpan to, CancellationToken cancel)
    {
        var frames = new List<Frame>();
        Run("ffmpeg", new[]
        {
            "-hide_banner", "-nostats", "-v", "info", "-copyts",
            "-ss", Seconds(from), "-i", path, "-to", Seconds(to),
            "-map", "0:v:0", "-vf", "showinfo",
            "-max_error_rate", "1", // the window is mostly damage: by default ffmpeg gives up above 2/3 bad frames
            "-f", "null", "-",
        }, line =>
        {
            var m = ShowinfoTime.Match(line);
            if (m.Success && TryTime(m.Groups[1].Value, out var t))
                frames.Add(new Frame(t, line.Contains("iskey:1", StringComparison.Ordinal)));
        }, cancel);
        return frames;
    }

    public TimeSpan? KeyFrameFrom(string path, TimeSpan at, CancellationToken cancel)
    {
        TimeSpan? found = null;
        Run("ffprobe", new[]
        {
            "-v", "quiet", "-select_streams", "v:0", // quiet: the damaged packets make the parser complain a lot
            // to the end: seeking lands on the key frame before the damage, a fixed length may not reach past it.
            // Only packets are listed, nothing is decoded - quick even for hours.
            "-read_intervals", Seconds(at) + "%",
            "-show_entries", "packet=pts_time,flags", "-of", "csv=p=0", path,
        }, line =>
        {
            var cells = line.Trim().Split(',');
            if (cells.Length < 2 || !cells[1].Contains('K') || !TryTime(cells[0], out var time)) return;
            if (time < at - Hair) return;
            if (found is null || time < found) found = time;
        }, cancel);
        return found;
    }

    public void Copy(string path, TimeSpan from, TimeSpan length, string output, CancellationToken cancel)
    {
        var args = new List<string> { "-hide_banner", "-nostats", "-v", "error", "-y" };
        if (from > TimeSpan.Zero) args.AddRange(new[] { "-ss", Seconds(from + Hair) });
        args.AddRange(new[]
        {
            "-i", path, "-t", Seconds(length),
            "-map", "0:v:0", "-map", "0:a?", "-c", "copy", "-avoid_negative_ts", "make_zero", output,
        });
        Run("ffmpeg", args, _ => { }, cancel);
    }

    public void Join(IReadOnlyList<string> parts, string output, CancellationToken cancel)
    {
        string list = output + ".parts.txt";
        var text = new StringBuilder();
        foreach (var part in parts) text.Append("file '").Append(Path.GetFullPath(part).Replace("'", @"'\''")).Append("'\n");
        File.WriteAllText(list, text.ToString());

        Run("ffmpeg", new[]
        {
            "-hide_banner", "-nostats", "-v", "error", "-y",
            "-f", "concat", "-safe", "0", "-i", list,
            "-map", "0", "-c", "copy", "-movflags", "+faststart", output,
        }, _ => { }, cancel);
    }

    public long EncodeConstantRate(string path, string output, MediaInfo info, string encoder, IProgress<double>? progress, CancellationToken cancel)
    {
        long duplicated = 0;
        var quality = encoder == FreezeRepair.Nvidia
            ? new[] { "-preset", NvidiaPreset, "-rc", "vbr", "-cq", NvidiaQuality, "-b:v", "0" }
            : new[] { "-crf", ProcessorQuality, "-preset", ProcessorPreset };

        Run("ffmpeg", new[]
        {
            "-hide_banner", "-nostats", "-v", "error", "-y",
            "-progress", "pipe:1", "-stats_period", ProgressPeriod,
            "-i", path,
            "-map", "0:v:0", "-map", "0:a?",
            "-fps_mode", "cfr", "-r", info.FrameRate,
            "-c:v", encoder,
        }.Concat(quality).Concat(new[]
        {
            "-pix_fmt", "yuv420p",
            // The sound is copied, damage and all. Decoding damaged sound makes ffmpeg 8 stop halfway
            // (exit code -22), so it cannot simply be encoded again. Lada copies the sound too.
            "-c:a", "copy",
            "-max_error_rate", "1", // a short video can be damaged more than 2/3, then ffmpeg would give up
            "-movflags", "+faststart",
            output,
        }).ToList(), line =>
        {
            line = line.Trim();
            if (Progress(line, info.Duration, progress)) return;
            if (line.StartsWith("dup_frames=", StringComparison.Ordinal)
                && long.TryParse(line["dup_frames=".Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out long n))
                duplicated = n;
        }, cancel);
        return duplicated;
    }

    /// <summary>Runs ffmpeg or ffprobe and fails with its last lines when it does not end with code 0.</summary>
    private void Run(string program, IReadOnlyList<string> args, Action<string> onLine, CancellationToken cancel)
    {
        var (exitCode, tail) = _runner.Run(Program(program), args, onLine, cancel);
        if (exitCode != 0) throw new InvalidOperationException($"{program} failed (exit code {exitCode}). {tail}");
    }

    private string Program(string name) => string.IsNullOrWhiteSpace(_folder) ? name : Path.Combine(_folder, name);

    /// <summary>Reports ffmpeg's "-progress pipe:1" lines as a share of <paramref name="duration"/>.</summary>
    private static bool Progress(string line, TimeSpan duration, IProgress<double>? progress)
    {
        if (!line.StartsWith("out_time_us=", StringComparison.Ordinal)) return false;
        if (long.TryParse(line["out_time_us=".Length..], NumberStyles.Integer, CultureInfo.InvariantCulture, out long us) && us >= 0 && duration > TimeSpan.Zero)
            progress?.Report(Math.Min(1, TimeSpan.FromTicks(us * 10) / duration));
        return true;
    }

    private static string Seconds(TimeSpan t) => t.TotalSeconds.ToString("0.######", CultureInfo.InvariantCulture);

    private static bool TryTime(string text, out TimeSpan time)
    {
        bool ok = double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double s);
        time = ok ? TimeSpan.FromSeconds(s) : TimeSpan.Zero;
        return ok;
    }

    private static string? Text(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() : null;

    private static int Int(JsonElement e, string name) =>
        e.TryGetProperty(name, out var v) && v.ValueKind == JsonValueKind.Number && v.TryGetInt32(out int i) ? i : 0;

    private static double Number(string? s) =>
        double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double d) ? d : 0;

    private static double Rate(string rate)
    {
        var parts = rate.Split('/');
        double n = Number(parts[0]), d = parts.Length > 1 ? Number(parts[1]) : 1;
        return d > 0 ? n / d : 0;
    }
}
