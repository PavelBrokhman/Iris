using static Iris.Core.Tests.Video;

namespace Iris.Core.Tests;

public class DiagnosisTests
{
    private static readonly DamageMessage Message = new(S(0), "a");

    private static CheckReport Report(double check, params (double From, double To)[] places) =>
        new("v.mp4", S(100), S(check), 0, places.Select(p => new DamagedPlace(S(p.From), S(p.To), [Message])).ToList(), "h264");

    [Fact]
    public void Nothing_found_needs_no_repair()
    {
        var d = new Diagnosis(Report(100));

        Assert.Equal(DamageKind.None, d.Kind);
        Assert.Empty(d.Repairs);
        Assert.Equal("No damage found.", d.Summary);
    }

    [Fact]
    public void Places_with_a_whole_video_after_them_suit_cut_and_freeze()
    {
        var d = new Diagnosis(Report(100, (40, 46)));

        Assert.Equal(DamageKind.Places, d.Kind);
        Assert.Equal(new[] { "cut", "freeze" }, d.Repairs);
        Assert.Null(d.WholeUntil);
        Assert.Equal("1 damaged place(s): 00:40 - 00:46.", d.Summary);
    }

    [Fact]
    public void A_decoder_that_stopped_early_means_damage_to_the_end_and_only_cut()
    {
        var d = new Diagnosis(Report(60, (55, 60)));

        Assert.Equal(DamageKind.ToTheEnd, d.Kind);
        Assert.Equal(S(55), d.WholeUntil);
        Assert.Equal(new[] { "cut" }, d.Repairs);
        Assert.Equal("Whole up to 00:55, damaged from there to the end.", d.Summary);
    }

    [Fact]
    public void Another_codec_than_h264_is_named()
    {
        var report = Report(100, (40, 46)) with { Codec = "hevc" };

        var d = new Diagnosis(report);

        Assert.Contains(d.Lines, l => l.Contains("hevc"));
    }
}
