using static Iris.Core.Tests.Video;

namespace Iris.Core.Tests;

public class CompareTests
{
    private static VideoDetails File(string path, double seconds, int height, double rate) =>
        new(path, S(seconds), height * 16 / 9, height, "h264", "High", 0, rate, rate, 2_000_000, 100_000_000, "aac");

    [Fact]
    public void Two_lengths_within_two_seconds_are_the_same_length()
    {
        var c = new Comparison(new(File("a", 10515, 720, 30), 1800, 1800), new(File("b", 10514, 720, 30), 1800, 1800));

        Assert.True(c.SameLength);
    }

    [Fact]
    public void Different_lengths_are_said()
    {
        var c = new Comparison(new(File("a", 600, 720, 30), 1800, 1800), new(File("b", 700, 720, 30), 1800, 1800));

        Assert.False(c.SameLength);
        Assert.Contains(c.Lines, l => l.Contains("not the same cut"));
    }

    [Fact]
    public void Repeated_frames_lose_to_real_ones()
    {
        // Nevestka4.mp4 against vid_720p (7).mp4: the same film and size; the new one 30 real frames/s, the old one repeats
        var old = new CompareSide(File("old", 10515, 720, 30), 1800, 1200);
        var fresh = new CompareSide(File("new", 10515, 720, 30), 1800, 1800);

        var c = new Comparison(old, fresh);

        Assert.Equal(1, c.Better);
        Assert.Contains(c.Lines, l => l.Contains("20.0") && l.Contains("30.0"));
    }

    [Fact]
    public void A_bigger_picture_wins_when_the_frames_are_alike()
    {
        var c = new Comparison(new(File("a", 600, 1080, 30), 1800, 1800), new(File("b", 600, 720, 30), 1800, 1800));

        Assert.Equal(0, c.Better);
    }

    [Fact]
    public void Size_against_real_frames_is_no_clear_winner()
    {
        var c = new Comparison(new(File("a", 600, 1080, 30), 1800, 1200), new(File("b", 600, 720, 30), 1800, 1800));

        Assert.Null(c.Better);
    }

    [Fact]
    public void Says_that_damage_is_not_looked_for()
    {
        // damaged-h264-no-frames.mp4 against its frozen copy: after a seek into the hole ffmpeg gave 24 frames/s - the numbers do not see damage
        var c = new Comparison(new(File("a", 480, 720, 24), 1440, 1440), new(File("b", 480, 720, 24), 1440, 1));

        Assert.Contains(c.Lines, l => l.Contains("iris check"));
    }

    [Fact]
    public void A_piece_where_nothing_decodes_is_said_and_does_not_count()
    {
        // damaged-h264-no-frames.mp4 against its frozen copy: the middle minute is the hole
        var c = new Comparison(new(File("a", 480, 720, 24), 0, 0), new(File("b", 480, 720, 24), 1440, 50));

        Assert.Contains(c.Lines, l => l.StartsWith("Real frames/s") && l.Contains("none decoded"));
        Assert.Null(c.Better);
    }

    [Fact]
    public void Compare_takes_the_same_piece_of_both_and_stills_from_the_same_moments()
    {
        var tools = new FakeTools();
        tools.AllDetails["a"] = File("a", 400, 720, 30);
        tools.AllDetails["b"] = File("b", 401, 1080, 30);
        tools.Samples["a"] = (1800, 1800);
        tools.Samples["b"] = (1800, 1800);
        var disk = new FakeDisk();

        var c = new Compare(tools, disk).Run("a", "b", "stills", CancellationToken.None);

        Assert.Equal(new[] { ("a", S(170), S(60)), ("b", S(170), S(60)) }, tools.SampleCalls);
        Assert.Equal(new[] { S(100), S(200), S(300) }, tools.Stills.Where(s => s.Path == "a").Select(s => s.At));
        Assert.Equal(6, tools.Stills.Count);
        Assert.Contains(Path.Combine("stills", "2-b.png"), tools.Stills.Select(s => s.Output));
        Assert.Equal(new[] { "stills" }, disk.Created);
        Assert.Equal(6, c.Stills.Count);
    }

    [Fact]
    public void Without_a_stills_folder_no_still_is_written()
    {
        var tools = new FakeTools();
        tools.AllDetails["a"] = File("a", 400, 720, 30);
        tools.AllDetails["b"] = File("b", 400, 720, 30);
        tools.Samples["a"] = (1800, 1800);
        tools.Samples["b"] = (1800, 1800);

        new Compare(tools, new FakeDisk()).Run("a", "b", null, CancellationToken.None);

        Assert.Empty(tools.Stills);
    }
}
