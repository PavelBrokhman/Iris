using static Iris.Core.Tests.Video;

namespace Iris.Core.Tests;

public class CutRepairTests
{
    private const string Source = "v.mp4", Output = "v.cut.mp4", Work = "work";

    private readonly FakeTools _tools = new();
    private readonly FakeDisk _disk = new();

    public CutRepairTests()
    {
        _tools.Infos[Source] = new MediaInfo(Source, S(100), "25", 25, "h264");
    }

    private RepairResult Repair(CheckReport report) =>
        new CutRepair(new DecodeCheck(_tools), _tools, _disk, Work).Repair(Source, Output, null, CancellationToken.None, report);

    private static CheckReport Report(double check, params (double From, double To)[] places) =>
        new(Source, S(100), S(check), 0, places.Select(p => new DamagedPlace(S(p.From), S(p.To), [new DamageMessage(S(p.From), "a")])).ToList(), "h264");

    private void Joined(double seconds) =>
        _tools.Infos[System.IO.Path.Combine(Work, "joined.mp4")] = new MediaInfo("joined", S(seconds), "25", 25, "h264");

    [Fact]
    public void Nothing_damaged_writes_nothing()
    {
        var result = Repair(Report(100));

        Assert.False(result.Changed);
        Assert.Empty(_tools.Copies);
        Assert.Empty(_disk.Moves);
    }

    [Fact]
    public void A_place_in_the_middle_is_cut_from_the_key_frame_before_the_hole_to_the_key_frame_after_it()
    {
        // frames to 40 s, a hole to 46 s, key frames every 2 s; the first key frame after the hole is at 48 s
        _tools.Window = (from, to) => Run(from.TotalSeconds, 40).Concat(Run(46, to.TotalSeconds)).ToList();
        _tools.KeyFrame = at => at <= S(48) ? S(48) : null;
        Joined(90);

        var result = Repair(Report(100, (39.96, 46)));

        Assert.Equal(new[] { (S(0), S(38)), (S(48), S(52)) }, _tools.Copies.Select(c => (c.From, c.Length)));
        var join = Assert.Single(_tools.Joins);
        Assert.Equal(_tools.Copies.Select(c => c.Output), join.Parts);
        Assert.Equal((join.Output, Output), Assert.Single(_disk.Moves));
        Assert.True(result.Changed);
        Assert.Equal("Cut out 1 piece(s), 00:10.00 in all: 00:38.00 - 00:48.00.", result.Summary);
        Assert.Equal(new[] { Work }, _disk.Deleted);
    }

    [Fact]
    public void The_end_of_a_place_not_found_stops_with_an_error_and_writes_nothing()
    {
        // no frame comes after 40 s in the window, though the check read the video to the end
        _tools.Window = (from, _) => Run(from.TotalSeconds, 40).ToList();

        var error = Assert.Throws<InvalidOperationException>(() => Repair(Report(100, (39.96, 46))));

        Assert.Contains("was not found", error.Message);
        Assert.Empty(_tools.Copies);
        Assert.Empty(_disk.Moves);
    }

    [Fact]
    public void Damage_to_the_end_keeps_the_whole_start()
    {
        _tools.Window = (from, _) => Run(from.TotalSeconds, 55).ToList();
        Joined(54);

        var result = Repair(Report(60, (55, 60)));

        Assert.Equal(new[] { (S(0), S(54)) }, _tools.Copies.Select(c => (c.From, c.Length)));
        Assert.Equal("Kept the whole start up to 00:54.00; from there the video was damaged to the end.", result.Summary);
    }

    [Fact]
    public void A_joined_video_of_the_wrong_length_is_not_delivered()
    {
        _tools.Window = (from, to) => Run(from.TotalSeconds, 40).Concat(Run(46, to.TotalSeconds)).ToList();
        _tools.KeyFrame = _ => S(48);
        Joined(70); // ffmpeg stopped joining on a bad packet

        var error = Assert.Throws<InvalidOperationException>(() => Repair(Report(100, (39.96, 46))));

        Assert.Contains("01:10.00 long instead of 01:30.00", error.Message);
        Assert.Empty(_disk.Moves);
        Assert.Equal(new[] { Work }, _disk.Deleted);
    }

    [Fact]
    public void An_existing_result_is_not_overwritten()
    {
        _disk.Files.Add(Output);

        Assert.Throws<IOException>(() => Repair(Report(100, (39.96, 46))));
        Assert.Empty(_tools.Copies);
    }
}
