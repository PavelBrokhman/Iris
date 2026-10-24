using static Iris.Core.Tests.Video;

namespace Iris.Core.Tests;

public class DecodeCheckTests
{
    private const string Path = "v.mp4";

    private static CheckReport Check(IReadOnlyList<TimeSpan> frames, params Complaint[] complaints)
    {
        var tools = new FakeTools { Log = new DecodeLog(frames, complaints, 0) };
        tools.Infos[Path] = new MediaInfo(Path, S(100), "25", 25, "h264");
        return new DecodeCheck(tools).Check(Path, null, CancellationToken.None);
    }

    private static List<TimeSpan> Times(params (double From, double To)[] runs) =>
        runs.SelectMany(r => Run(r.From, r.To)).Select(f => f.At).ToList();

    [Fact]
    public void A_whole_video_has_no_places_and_is_read_to_the_end()
    {
        var report = Check(Times((0, 100)));

        Assert.Empty(report.Places);
        Assert.True(report.ReadToTheEnd);
        Assert.False(report.Damaged);
        Assert.Equal("h264", report.Codec);
    }

    [Fact]
    public void A_place_runs_from_the_last_frame_before_it_to_the_first_frame_after_it()
    {
        // frames to 40 s, a hole, frames again from 46 s; the complaints come after the frame at 39.96 s
        var frames = Times((0, 40), (46, 100));
        int before = frames.Count(t => t < S(40));

        var report = Check(frames, new Complaint("[h264] Invalid NAL unit size", before), new Complaint("[h264] no frame!", before));

        var place = Assert.Single(report.Places);
        Assert.Equal(S(39.96), place.From);
        Assert.Equal(S(46), place.To);
        Assert.Equal(new[] { "[h264] Invalid NAL unit size", "[h264] no frame!", "No frame for 00:06.04" }, place.Messages.Select(m => m.Text));
    }

    [Fact]
    public void Complaints_more_than_three_seconds_apart_are_separate_places()
    {
        var frames = Times((0, 100));
        int at10 = frames.Count(t => t <= S(10)), at20 = frames.Count(t => t <= S(20));

        var report = Check(frames, new Complaint("a", at10), new Complaint("b", at20));

        Assert.Equal(2, report.Places.Count);
        Assert.Equal(S(10), report.Places[0].From);
        Assert.Equal(S(20), report.Places[1].From);
    }

    [Fact]
    public void A_silent_hole_right_after_the_complaints_belongs_to_their_place()
    {
        // damaged-h264-nal-short.mp4: complaints at 01:00 - 01:01, frames until 01:01.6, then none until 04:07.1
        var frames = Times((0, 61.6), (247.1, 300));
        int before = frames.Count(t => t < S(60)), after = frames.Count(t => t < S(61));

        var report = Check(frames, new Complaint("a", before), new Complaint("b", after));

        var place = Assert.Single(report.Places);
        Assert.Equal(S(59.96), place.From);
        Assert.Equal(S(247.12), place.To);
    }

    [Fact]
    public void A_hole_in_the_frames_without_complaints_is_a_damaged_place()
    {
        var report = Check(Times((0, 30), (40, 100)));

        var place = Assert.Single(report.Places);
        Assert.Equal(S(29.96), place.From);
        Assert.Equal(S(40), place.To);
        Assert.Contains("No frame", Assert.Single(place.Messages).Text);
    }

    [Fact]
    public void Ordinary_gaps_between_frames_are_no_hole()
    {
        // a variable frame rate: some frames last a little longer
        var frames = Times((0, 50), (50.2, 100));

        Assert.Empty(Check(frames).Places);
    }

    [Fact]
    public void Damage_with_no_frame_after_it_goes_to_the_last_frame_read()
    {
        var frames = Times((0, 60));

        var report = Check(frames, new Complaint("a", frames.Count));

        Assert.Equal(S(59.96), report.Checked);
        Assert.False(report.ReadToTheEnd);
        Assert.Equal(S(59.96), Assert.Single(report.Places).To);
    }
}
