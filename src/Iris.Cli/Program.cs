using Iris.Core;

// iris check <video> [--ffmpeg <folder>]
// iris repair cut|freeze <video> [--output <file>] [--encoder libx264|h264_nvenc] [--ffmpeg <folder>]
// iris diagnose <video> [--ffmpeg <folder>]
// iris compare <video> <video> [--stills <folder>] [--ffmpeg <folder>]
// Exit codes: 0 - whole / repaired / told, 1 - damaged, 2 - error, 3 - stopped.

const int Whole = 0, Damaged = 1, Error = 2, Stopped = 3;
const int SamplesPerPlace = 3;

if (args.Length < 2 || args[0] is "-h" or "--help" || (args[0] is "repair" or "compare" && args.Length < 3))
{
    Console.WriteLine("iris check <video> [--ffmpeg <folder>]");
    Console.WriteLine("  decodes the whole picture and lists the damaged places, with the repairs that suit");
    Console.WriteLine("iris repair cut|freeze <video> [--output <file>] [--encoder libx264|h264_nvenc] [--ffmpeg <folder>]");
    Console.WriteLine("  cut     cuts the damaged places out, nothing is encoded again; the picture and sound jump there");
    Console.WriteLine("  freeze  encodes the video again; a damaged place shows the last good frame, the length stays");
    Console.WriteLine("  The video itself is only read; the result goes next to it: <name>.cut.mp4, <name>.frozen.mp4.");
    Console.WriteLine("iris diagnose <video> [--ffmpeg <folder>]");
    Console.WriteLine("  what the file says about itself - codec, key frames, frame rate - and what it means for players and tools");
    Console.WriteLine("iris compare <video> <video> [--stills <folder>] [--ffmpeg <folder>]");
    Console.WriteLine("  the same film? which is better on the numbers? --stills writes frames of both from the same moments");
    return args.Length == 0 || args[0] is "-h" or "--help" ? Whole : Error;
}

string? Option(string name)
{
    int i = Array.IndexOf(args, name);
    return i >= 0 && i + 1 < args.Length ? args[i + 1] : null;
}

Console.OutputEncoding = System.Text.Encoding.UTF8; // ×, ←, → in the findings
using var cancel = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) => { e.Cancel = true; cancel.Cancel(); };

IVideoTools tools = new FfmpegTools(new ProcessRunner(), Option("--ffmpeg"));
IVideoCheck check = new DecodeCheck(tools);
int shown = -1;
var progress = new Progress<double>(p =>
{
    int percent = (int)(p * 100);
    if (percent == shown) return;
    shown = percent;
    Console.Error.Write($"\r{(args[0] == "repair" ? "Repairing" : "Checking")}… {percent}%   ");
});

try
{
    if (args[0] == "check")
    {
        var report = check.Check(args[1], progress, cancel.Token);
        Console.Error.WriteLine("\r                    ");
        return Print(report);
    }
    if (args[0] == "repair")
    {
        string path = args[2];
        // a folder per run next to iris.exe, so that two runs at once do not share pieces
        string work = Path.Combine(AppContext.BaseDirectory, "work", Guid.NewGuid().ToString("N"));
        var repair = new RepairFactory(check, tools, new Disk(), work).Create(args[1], Option("--encoder"));
        string output = Option("--output") ?? Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!,
            Path.GetFileNameWithoutExtension(path) + "." + repair.ResultWord + ".mp4");
        if (string.Equals(Path.GetFullPath(output), Path.GetFullPath(path), StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("The result cannot replace the video itself.");

        var result = repair.Repair(path, output, progress, cancel.Token);
        Console.Error.WriteLine("\r                    ");
        Console.WriteLine(result.Summary);
        if (result.Changed) Console.WriteLine("Written: " + result.OutputPath);
        return Whole;
    }
    if (args[0] == "diagnose")
    {
        foreach (var finding in new Diagnose(tools).Run(args[1], cancel.Token).Findings)
        {
            Console.WriteLine(finding.Fact);
            if (finding.Meaning is { } meaning) Console.WriteLine("    " + meaning);
        }
        return Whole;
    }
    if (args[0] == "compare")
    {
        var comparison = new Compare(tools, new Disk()).Run(args[1], args[2], Option("--stills"), cancel.Token);
        Console.WriteLine($"1: {args[1]}");
        Console.WriteLine($"2: {args[2]}");
        foreach (var line in comparison.Lines) Console.WriteLine(line);
        if (comparison.Stills.Count > 0) Console.WriteLine("Stills: " + Path.GetDirectoryName(comparison.Stills[0]));
        return Whole;
    }
    Console.Error.WriteLine($"Unknown command '{args[0]}': use check, repair, diagnose or compare.");
    return Error;
}
catch (OperationCanceledException)
{
    Console.Error.WriteLine("\rStopped.          ");
    return Stopped;
}
catch (Exception ex)
{
    Console.Error.WriteLine("\r" + ex.Message);
    return Error;
}

static int Print(CheckReport report)
{
    var diagnosis = new Diagnosis(report);

    Console.WriteLine($"{Path.GetFileName(report.Path)} ({report.Codec}): length {Clock.Short(report.Duration)}, read up to {Clock.Short(report.Checked)}");
    foreach (var line in diagnosis.Lines) Console.WriteLine(line);
    if (diagnosis.Repairs.Count > 0) Console.WriteLine("Repairs that suit: " + string.Join(", ", diagnosis.Repairs));

    foreach (var place in report.Places)
    {
        Console.WriteLine($"Complaints of the decoder at {Clock.Short(place.From)}: {place.Messages.Count}, for example:");
        foreach (var text in place.Messages.Select(m => m.Text).Distinct().Take(SamplesPerPlace))
            Console.WriteLine("    " + text);
    }
    return diagnosis.Kind == DamageKind.None && !report.Damaged ? Whole : Damaged;
}
