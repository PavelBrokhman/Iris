using System.ComponentModel;
using System.Diagnostics;

namespace Iris.Core;

/// <summary>Runs a program to the end.</summary>
public interface IProcessRunner
{
    /// <summary>
    /// Every line the program prints (stdout and stderr) goes to <paramref name="onLine"/>. On cancel the
    /// program is stopped and <see cref="OperationCanceledException"/> is thrown.
    /// Returns the exit code and the last lines of output, for error messages.
    /// </summary>
    (int ExitCode, string Tail) Run(string program, IReadOnlyList<string> args, Action<string> onLine, CancellationToken cancel);
}

public sealed class ProcessRunner : IProcessRunner
{
    /// <summary>How many of the last lines an error message quotes.</summary>
    private const int TailLines = 12;

    /// <summary>How often a running program is asked whether it ended, to notice a cancel, in milliseconds.</summary>
    private const int PollMs = 200;

    public (int ExitCode, string Tail) Run(string program, IReadOnlyList<string> args, Action<string> onLine, CancellationToken cancel)
    {
        var psi = new ProcessStartInfo(program)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
        };
        foreach (var a in args) psi.ArgumentList.Add(a);

        var tail = new Queue<string>();
        using var process = new Process { StartInfo = psi };

        void OnData(object _, DataReceivedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Data)) return;
            lock (tail)
            {
                tail.Enqueue(e.Data.Trim());
                if (tail.Count > TailLines) tail.Dequeue();
                onLine(e.Data);
            }
        }
        process.OutputDataReceived += OnData;
        process.ErrorDataReceived += OnData;

        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            throw new InvalidOperationException($"Cannot start {program}: {ex.Message}");
        }
        process.StandardInput.Close(); // a program that asks "press any key" fails at once instead of waiting forever
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            while (!process.WaitForExit(PollMs))
                cancel.ThrowIfCancellationRequested();
            process.WaitForExit(); // let the output handlers finish
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(entireProcessTree: true); } catch { /* already gone */ }
            throw;
        }

        lock (tail) return (process.ExitCode, string.Join(Environment.NewLine, tail));
    }
}
