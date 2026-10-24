using static Iris.Core.Tests.Video;

namespace Iris.Core.Tests;

public class FreezeRepairTests
{
    private const string Source = "v.mp4", Output = "v.frozen.mp4", Work = "work";

    [Fact]
    public void Encodes_again_at_the_videos_rate_and_tells_how_many_frames_were_copies()
    {
        var tools = new FakeTools { Duplicated = 150 };
        tools.Infos[Source] = new MediaInfo(Source, S(100), "30000/1001", 29.97, "h264");
        var disk = new FakeDisk();

        var result = new FreezeRepair(tools, disk, Work, FreezeRepair.Nvidia).Repair(Source, Output, null, CancellationToken.None);

        var encode = Assert.Single(tools.Encodes);
        Assert.Equal(("h264_nvenc", "30000/1001"), (encode.Encoder, encode.FrameRate));
        Assert.Equal((encode.Output, Output), Assert.Single(disk.Moves));
        Assert.StartsWith("Encoded again; 150 frame(s) were copies of the frame before them (about 00:05.00 in all).", result.Summary);
        Assert.Equal(new[] { Work }, disk.Deleted);
    }

    [Fact]
    public void Factory_knows_cut_and_freeze_and_nothing_else()
    {
        var tools = new FakeTools();
        var factory = new RepairFactory(new DecodeCheck(tools), tools, new FakeDisk(), Work);

        Assert.IsType<CutRepair>(factory.Create("cut"));
        Assert.IsType<FreezeRepair>(factory.Create("FREEZE"));
        Assert.Throws<ArgumentException>(() => factory.Create("glue"));
    }
}
