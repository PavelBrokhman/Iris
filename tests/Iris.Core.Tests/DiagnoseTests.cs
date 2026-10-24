using static Iris.Core.Tests.Video;

namespace Iris.Core.Tests;

public class DiagnoseTests
{
    /// <summary>An ordinary H.264 file: no B-frames, a constant 24 frames/s.</summary>
    private static VideoDetails Plain(double seconds = 600) =>
        new("v.mp4", S(seconds), 1280, 720, "h264", "High", 0, 24, 24, 2_000_000, 150_000_000, "aac");

    private static IReadOnlyList<TimeSpan> KeysEvery(double seconds, double length = 600) =>
        Enumerable.Range(0, (int)(length / seconds)).Select(i => S(i * seconds)).ToList();

    private static Finding Find(FileDiagnosis d, string word) => Assert.Single(d.Findings, f => f.Fact.Contains(word));

    [Fact]
    public void The_first_finding_names_the_file()
    {
        var d = new FileDiagnosis(Plain(), KeysEvery(2));

        Assert.Equal("h264 High, 1280×720, 24 frames/s, 2000 kbit/s, 10:00, 14400 frames, sound aac", d.Findings[0].Fact);
        Assert.Null(d.Findings[0].Meaning);
    }

    [Fact]
    public void Rare_key_frames_mean_a_blank_thumbnail_and_slow_seeking()
    {
        var d = new FileDiagnosis(Plain(), KeysEvery(10));

        var key = Find(d, "key frame");
        Assert.Equal("A key frame every 10.0 s (the longest gap 10.0 s)", key.Fact);
        Assert.Contains("blank thumbnail", key.Meaning);
    }

    [Fact]
    public void Key_frames_every_two_seconds_give_a_thumbnail()
    {
        var key = Find(new FileDiagnosis(Plain(), KeysEvery(2)), "key frame");

        Assert.Contains("shows a picture", key.Meaning);
    }

    [Fact]
    public void Two_seconds_of_frames_at_29_97_count_as_two_seconds()
    {
        // Doch'.demosaic.mp4: a key frame every 60 frames = 2.002 s
        var key = Find(new FileDiagnosis(Plain(), KeysEvery(60 / 29.97)), "key frame");

        Assert.Contains("shows a picture", key.Meaning);
    }

    [Fact]
    public void Between_what_was_checked_nothing_is_claimed_about_thumbnails()
    {
        var key = Find(new FileDiagnosis(Plain(), KeysEvery(5)), "key frame");

        Assert.Contains("not checked", key.Meaning);
    }

    [Fact]
    public void HEVC_with_B_frames_warns_about_VLC()
    {
        var details = Plain() with { Codec = "hevc", Profile = "Main", BFrames = 2 };

        var b = Find(new FileDiagnosis(details, KeysEvery(2.5)), "B-frames");

        Assert.Equal("HEVC with B-frames (up to 2 in a row)", b.Fact);
        Assert.Contains("VLC", b.Meaning);
    }

    [Fact]
    public void H264_with_B_frames_is_only_a_fact()
    {
        var details = Plain() with { BFrames = 2 };

        Assert.DoesNotContain(new FileDiagnosis(details, KeysEvery(2)).Findings, f => f.Fact.Contains("B-frames") && f.Meaning is not null);
    }

    [Fact]
    public void A_reported_rate_of_90000_is_named_with_the_real_one_and_Lada()
    {
        // vfr-frame-rate-90000.mp4
        var details = Plain(180) with { ReportedRate = 90000, AverageRate = 23.99 };

        var rate = Find(new FileDiagnosis(details, KeysEvery(2, 180)), "frame rate");

        Assert.Equal("Variable frame rate: reports 90000 frames/s, really 23.99", rate.Fact);
        Assert.Contains("Lada", rate.Meaning);
    }

    [Fact]
    public void Lada_is_named_only_when_its_estimate_would_never_come()
    {
        // damaged-timestamps.mp4: reports 59.94, really 29.45, 3534 frames - Lada waits for 59.94 × 15 = 899 frames, it gets them
        var details = Plain(120) with { ReportedRate = 59.94, AverageRate = 29.45 };

        var rate = Find(new FileDiagnosis(details, KeysEvery(3, 120)), "frame rate");

        Assert.DoesNotContain("Lada", rate.Meaning);
    }

    [Fact]
    public void A_key_frame_gap_far_longer_than_the_others_points_to_damage()
    {
        // damaged-h264-no-frames.mp4: every 3 s, but 158 s where the data is damaged
        var keys = KeysEvery(3, 129).Concat(KeysEvery(3, 180).Select(k => k + S(287))).ToList();

        var key = Find(new FileDiagnosis(Plain(480), keys), "key frame");

        Assert.Contains("iris check", key.Meaning);
    }

    [Fact]
    public void A_slightly_variable_rate_is_named_without_Lada()
    {
        var details = Plain() with { ReportedRate = 30, AverageRate = 29.45 };

        var rate = Find(new FileDiagnosis(details, KeysEvery(2)), "frame rate");

        Assert.DoesNotContain("Lada", rate.Meaning);
        Assert.Contains("repeated", rate.Meaning);
    }

    [Fact]
    public void A_constant_rate_says_nothing_about_it()
    {
        Assert.DoesNotContain(new FileDiagnosis(Plain(), KeysEvery(2)).Findings, f => f.Fact.Contains("frame rate"));
    }

    [Fact]
    public void Diagnose_asks_the_tools_for_the_details_and_the_key_frames()
    {
        var tools = new FakeTools();
        tools.AllDetails["v.mp4"] = Plain();
        tools.Keys["v.mp4"] = KeysEvery(10);

        var d = new Diagnose(tools).Run("v.mp4", CancellationToken.None);

        Assert.Contains(d.Findings, f => f.Fact.StartsWith("A key frame every 10.0 s"));
    }
}
