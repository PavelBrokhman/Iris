using static Iris.Core.Tests.Video;

namespace Iris.Core.Tests;

/// <summary>What the lines of ffmpeg and ffprobe are read as. No ffmpeg runs here.</summary>
public class FfmpegToolsTests
{
    private readonly FakeRunner _runner = new();

    private FfmpegTools Tools() => new(_runner, null);

    [Fact]
    public void Decode_reads_frame_times_and_complaints_without_address_and_level()
    {
        _runner.Lines = (_, _) => new[]
        {
            "[Parsed_showinfo_0 @ 000001d2] [info] n:   0 pts:      0 pts_time:0       duration:512 iskey:1 type:I",
            "[Parsed_showinfo_0 @ 000001d2] [info] n:   1 pts:    512 pts_time:0.04    duration:512 iskey:0 type:P",
            "[h264 @ 000001aa0aa3e1c0] [error] Invalid NAL unit size (1234 > 567).",
            "[h264 @ 000001aa0aa3e1c0] [error] Last message repeated 3 times",
            "out_time_us=40000",
            "[Parsed_showinfo_0 @ 000001d2] [info] n:   2 pts:   1024 pts_time:0.08    duration:512 iskey:0 type:P",
        };

        var log = Tools().Decode("v.mp4", S(1), null, CancellationToken.None);

        Assert.Equal(new[] { S(0), S(0.04), S(0.08) }, log.Frames);
        var complaint = Assert.Single(log.Complaints);
        Assert.Equal(new Complaint("[h264] Invalid NAL unit size (1234 > 567).", 2), complaint);
    }

    [Fact]
    public void Complaints_of_the_null_output_are_not_about_the_video()
    {
        // damaged-timestamps.mp4 and vfr-frame-rate-90000.mp4: the decoder is silent, only the output that writes nothing complains
        _runner.Lines = (_, _) => new[]
        {
            "[null @ 0000020a] [error] Application provided invalid, non monotonically increasing dts to muxer in stream 0: 1897 >= 1897",
            "[vost#0:0/wrapped_avframe @ 0000020b] [error] Error submitting a packet to the muxer: Invalid argument",
        };

        var log = Tools().Decode("v.mp4", S(1), null, CancellationToken.None);

        Assert.Empty(log.Complaints);
    }

    [Fact]
    public void Decode_reads_only_the_picture_and_does_not_give_up_on_much_damage()
    {
        Tools().Decode("v.mp4", S(1), null, CancellationToken.None);

        var (program, args) = Assert.Single(_runner.Calls);
        Assert.Equal("ffmpeg", program);
        Assert.Contains("0:v:0", args);
        Assert.Equal("1", args[args.ToList().IndexOf("-max_error_rate") + 1]);
    }

    [Fact]
    public void Decode_reports_progress_as_a_share_of_the_length()
    {
        _runner.Lines = (_, _) => new[] { "out_time_us=2500000" };
        double last = -1;

        Tools().Decode("v.mp4", S(10), new SyncProgress(p => last = p), CancellationToken.None);

        Assert.Equal(0.25, last, 3);
    }

    [Fact]
    public void Frames_reads_times_and_key_frames()
    {
        _runner.Lines = (_, _) => new[]
        {
            "[Parsed_showinfo_0 @ 0000] n:   0 pts: 960000 pts_time:40       iskey:1 type:I",
            "[Parsed_showinfo_0 @ 0000] n:   1 pts: 961024 pts_time:40.04    iskey:0 type:P",
            "frame=  2 fps=0.0 q=-0.0 size=N/A time=00:00:40.04",
        };

        var frames = Tools().Frames("v.mp4", S(30), S(76), CancellationToken.None);

        Assert.Equal(new[] { new Frame(S(40), true), new Frame(S(40.04), false) }, frames);
    }

    [Fact]
    public void KeyFrameFrom_takes_the_first_key_packet_at_or_after_the_time()
    {
        _runner.Lines = (_, _) => new[] { "45.000000,K__", "46.000000,___", "48.000000,K__", "50.000000,K__" };

        var key = Tools().KeyFrameFrom("v.mp4", S(46), CancellationToken.None);

        Assert.Equal(S(48), key);
    }

    [Fact]
    public void KeyFrameFrom_none_to_the_end_is_null()
    {
        _runner.Lines = (_, _) => new[] { "45.000000,K__", "46.000000,___" };

        Assert.Null(Tools().KeyFrameFrom("v.mp4", S(46), CancellationToken.None));
    }

    [Fact]
    public void Copy_seeks_a_hair_after_the_key_frame_so_it_cannot_land_on_the_one_before()
    {
        Tools().Copy("v.mp4", S(48), S(52), "p1.mp4", CancellationToken.None);

        var args = Assert.Single(_runner.Calls).Args.ToList();
        Assert.Equal("48.001", args[args.IndexOf("-ss") + 1]);
        Assert.Equal("52", args[args.IndexOf("-t") + 1]);
        Assert.Equal("copy", args[args.IndexOf("-c") + 1]);
    }

    [Fact]
    public void Copy_from_the_start_does_not_seek()
    {
        Tools().Copy("v.mp4", S(0), S(38), "p0.mp4", CancellationToken.None);

        Assert.DoesNotContain("-ss", Assert.Single(_runner.Calls).Args);
    }

    [Fact]
    public void Probe_reads_length_rate_and_codec()
    {
        _runner.Lines = (_, _) => new[]
        {
            """{ "streams": [ { "codec_type": "audio", "codec_name": "aac" },""",
            """ { "codec_type": "video", "codec_name": "h264", "avg_frame_rate": "30000/1001", "r_frame_rate": "30000/1001", "duration": "100.5" } ],""",
            """ "format": { "duration": "101.0" } }""",
        };

        var info = Tools().Probe("v.mp4");

        Assert.Equal(new MediaInfo("v.mp4", S(100.5), "30000/1001", 30000 / 1001.0, "h264"), info with { Fps = 30000 / 1001.0 });
        Assert.Equal(29.97, info.Fps, 2);
    }

    [Fact]
    public void Details_reads_size_rates_B_frames_and_bit_rate()
    {
        _runner.Lines = (_, _) => new[]
        {
            """{ "streams": [ { "codec_type": "video", "codec_name": "hevc", "profile": "Main", "width": 1280, "height": 720,""",
            """ "has_b_frames": 2, "r_frame_rate": "90000/1", "avg_frame_rate": "2399/100", "duration": "180.0" },""",
            """ { "codec_type": "audio", "codec_name": "aac" } ],""",
            """ "format": { "duration": "180.1", "size": "118591003", "bit_rate": "5270000" } }""",
        };

        var d = Tools().Details("v.mp4");

        Assert.Equal(new VideoDetails("v.mp4", S(180), 1280, 720, "hevc", "Main", 2, 90000, 23.99, 5_270_000, 118_591_003, "aac"), d);
    }

    [Fact]
    public void KeyFrames_lists_the_key_packets()
    {
        _runner.Lines = (_, _) => new[] { "0.000000,K__", "0.041667,___", "2.500000,K__", "N/A,K__" };

        Assert.Equal(new[] { S(0), S(2.5) }, Tools().KeyFrames("v.mp4", CancellationToken.None));
    }

    [Fact]
    public void Repeats_counts_the_frames_in_and_those_kept()
    {
        _runner.Lines = (_, _) => new[]
        {
            "[Parsed_showinfo_0 @ 01] n:   0 pts:  0 pts_time:0    iskey:1",
            "[Parsed_showinfo_0 @ 01] n:   1 pts:  1 pts_time:0.04 iskey:0",
            "[Parsed_showinfo_0 @ 01] n:   2 pts:  2 pts_time:0.08 iskey:0",
            "frame=1",
            "frame=2",
            "progress=end",
            // ffmpeg's last summary on stderr, which may come after the progress on stdout
            "frame=    2 fps=0.0 q=-0.0 Lsize=N/A time=00:00:00.08 bitrate=N/A speed=8.57x",
        };

        Assert.Equal((3L, 2L), Tools().Repeats("v.mp4", S(10), S(60), CancellationToken.None));
        var args = Assert.Single(_runner.Calls).Args.ToList();
        Assert.Equal("showinfo,mpdecimate", args[args.IndexOf("-vf") + 1]);
        Assert.Equal("passthrough", args[args.IndexOf("-fps_mode") + 1]);
        Assert.Equal("1", args[args.IndexOf("-max_error_rate") + 1]); // the piece may be all damage
    }

    [Fact]
    public void A_failing_ffmpeg_is_an_error_with_its_last_lines()
    {
        _runner.ExitCode = 1;

        var error = Assert.Throws<InvalidOperationException>(() => Tools().Copy("v.mp4", S(0), S(1), "p.mp4", CancellationToken.None));

        Assert.Contains("exit code 1", error.Message);
        Assert.Contains("the last lines", error.Message);
    }

    /// <summary>Progress&lt;T&gt; reports on another thread; this one at once.</summary>
    private sealed class SyncProgress(Action<double> report) : IProgress<double>
    {
        public void Report(double value) => report(value);
    }
}
