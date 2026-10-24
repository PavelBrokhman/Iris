namespace Iris.Core;

/// <summary>What a check found, in short.</summary>
public enum DamageKind
{
    /// <summary>Read to the end, no complaints.</summary>
    None,

    /// <summary>Damaged places with a whole video after them.</summary>
    Places,

    /// <summary>From some place on nothing can be read to the end: only the start is whole.</summary>
    ToTheEnd,
}

/// <summary>
/// A <see cref="CheckReport"/> told for a person: what is damaged, and which repairs suit it.
/// A video damaged to the end gets only the cut (it keeps the whole start); freezing would make the rest one still picture.
/// </summary>
public sealed class Diagnosis
{
    /// <summary>The codec the repairs were tried on; with another one the result is to be looked at.</summary>
    public const string TriedCodec = "h264";

    public Diagnosis(CheckReport report)
    {
        Report = report;
        var places = report.Places.ToList();
        var lines = new List<string>();

        bool stoppedEarly = !report.ReadToTheEnd;
        bool lastReachesEnd = places.Count > 0 && report.Checked - places[^1].To < CheckReport.EndTolerance;
        if (stoppedEarly || lastReachesEnd)
        {
            Kind = DamageKind.ToTheEnd;
            WholeUntil = lastReachesEnd ? places[^1].From : report.Checked;
            if (lastReachesEnd) places.RemoveAt(places.Count - 1);
            Repairs = [CutRepair.RepairName];
        }
        else if (places.Count > 0)
        {
            Kind = DamageKind.Places;
            Repairs = [CutRepair.RepairName, FreezeRepair.RepairName];
        }
        else
            Kind = DamageKind.None;

        if (Kind == DamageKind.None)
            lines.Add("No damage found.");
        if (places.Count > 0)
        {
            lines.Add($"{places.Count} damaged place(s), the video goes on after each:");
            lines.AddRange(places.Select(p => $"  {Clock.Short(p.From)} - {Clock.Short(p.To)}"));
        }
        if (WholeUntil is { } until)
        {
            lines.Add($"Damaged from {Clock.Short(until)} to the end: {Clock.Short(report.Duration - until)} of {Clock.Short(report.Duration)} cannot be read.");
            lines.Add("Only the start is whole. The rest is not in the file - download the video again to get it.");
        }
        if (report.ExitCode != 0)
            lines.Add($"ffmpeg ended with exit code {report.ExitCode}.");
        if (Kind != DamageKind.None && report.Codec is { } codec && codec != TriedCodec)
            lines.Add($"The repairs were tried on H.264 videos only; this one is {codec} - look at the result.");
        Lines = lines;

        string named = string.Join(", ", places.Select(p => $"{Clock.Short(p.From)} - {Clock.Short(p.To)}"));
        Summary = Kind switch
        {
            DamageKind.None => "No damage found.",
            DamageKind.Places => $"{places.Count} damaged place(s): {named}.",
            _ => $"Whole up to {Clock.Short(WholeUntil!.Value)}, damaged from there to the end" +
                 (places.Count > 0 ? $"; {places.Count} damaged place(s) before that." : "."),
        };
    }

    public CheckReport Report { get; }

    public DamageKind Kind { get; }

    /// <summary><see cref="DamageKind.ToTheEnd"/>: where the damage to the end starts; null otherwise.</summary>
    public TimeSpan? WholeUntil { get; }

    /// <summary>The names of the repairs that suit (<see cref="IVideoRepair.Name"/>), the best first.</summary>
    public IReadOnlyList<string> Repairs { get; } = [];

    /// <summary>For a person: one line per finding.</summary>
    public IReadOnlyList<string> Lines { get; }

    /// <summary>For a person, in one short line.</summary>
    public string Summary { get; }
}
