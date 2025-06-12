using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;

public abstract class ConsoleCommandBase<TConfig>(string identifier) : IConsoleCommand
{
    protected IConsoleWriter Writer => ConsoleService.Writer;
    public string Identifier { get; } = identifier;
    public TConfig Configuration { get; private set; } = default!;
    protected virtual void SetConfiguration(TConfig config) => Configuration = config;
    public abstract RunResult Run(ICommandLineInput input);
    public virtual void OnInitialized() { }
    protected RunResult Ok(string message = "") => new RunResult(Identifier, true, message);
    protected RunResult Nok(string message = "") => new RunResult(Identifier, false, message);

    /// <summary>
    /// Executes a shell command and writes both stdout and stderr to the console.
    /// </summary>
    /// <param name="command">The command to run (e.g., "kubectl get pods")</param>
    /// <param name="heading">The heading to display above the output</param>

    protected void RunCommand(string command, string heading)
    {
        Writer.WriteHeadLine(heading);

        string shell, shellArgs;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            shell    = "cmd.exe";
            shellArgs = $"/c {command}";
        }
        else
        {
            shell    = "/bin/bash";
            shellArgs = $"-c \"{command}\"";
        }

        var psi = new ProcessStartInfo
        {
            FileName               = shell,
            Arguments              = shellArgs,
            RedirectStandardOutput = true,
            RedirectStandardError  = true,
            UseShellExecute        = false,
            CreateNoWindow         = true
        };

        using var proc = Process.Start(psi);
        if (proc == null)
        {
            Writer.WriteLine($"Failed to start process for: {command}");
            return;
        }
        var stdout = proc.StandardOutput.ReadToEnd();
        var stderr = proc.StandardError.ReadToEnd();
        proc.WaitForExit();
        
        if (!string.IsNullOrWhiteSpace(stdout)) Writer.WriteLine(stdout.TrimEnd());
        if (!string.IsNullOrWhiteSpace(stderr)) Writer.WriteLine(stderr.TrimEnd());
    }
}