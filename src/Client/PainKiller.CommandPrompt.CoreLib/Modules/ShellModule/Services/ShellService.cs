using System.Diagnostics;
using System.Text;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Contracts;

namespace PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;
public class ShellService : IShellService
{
    private static readonly Lazy<IShellService> Instance = new(() => new ShellService());
    private ShellService() { }
    public static IShellService Default => Instance.Value;
    public void OpenDirectory(string path)
    {
        var actualPath = ReplacePlaceholders(path);
        if (!Directory.Exists(actualPath)) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = actualPath,
            UseShellExecute = true,
            Verb = "open"
        });
    }

    public void OpenWithDefaultProgram(string path, string workingDirectory = "")
    {
        var actualPath = ReplacePlaceholders(path);

        Process.Start(new ProcessStartInfo
        {
            FileName = actualPath,
            WorkingDirectory = ReplacePlaceholders(workingDirectory),
            UseShellExecute = true,
            Verb = "open"
        });
    }

    public void Execute(string program, string args = "", string workingDirectory = "", bool waitForExit = false)
    {
        var psi = new ProcessStartInfo
        {
            FileName = ReplacePlaceholders(program),
            Arguments = args,
            WorkingDirectory = ReplacePlaceholders(workingDirectory),
            RedirectStandardOutput = !waitForExit,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = Process.Start(psi);
        if (waitForExit)
        {
            process!.WaitForExit();
            var output = process.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
        }
    }
    public string StartInteractiveProcess(string program, string args = "", string workingDirectory = "", bool waitForExit = true)
    {
        var output = new StringBuilder();

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = ReplacePlaceholders(program),
                Arguments = args,
                WorkingDirectory = ReplacePlaceholders(workingDirectory),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
            {
                Console.WriteLine("Failed to start process.");
                return "Process failed to start.";
            }

            if (waitForExit)
            {
                // Läs utdata och felmeddelanden i realtid
                output.AppendLine(process.StandardOutput.ReadToEnd());
                output.AppendLine(process.StandardError.ReadToEnd());
                process.WaitForExit();
            }
            else
            {
                // Läsa utdata utan att vänta på att processen ska avslutas
                process.OutputDataReceived += (sender, e) => 
                {
                    if (e.Data != null) 
                        output.AppendLine(e.Data);
                };
                process.ErrorDataReceived += (sender, e) => 
                {
                    if (e.Data != null) 
                        output.AppendLine($"ERROR: {e.Data}");
                };

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
            }
        }
        catch (Exception ex)
        {
            output.AppendLine($"Error executing {program}: {ex.Message}");
        }

        return output.ToString();
    }
    private static string ReplacePlaceholders(string input)
    {
        return input
            .Replace("$ROAMING$", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
            .Replace("%USERNAME%", Environment.UserName, StringComparison.OrdinalIgnoreCase);
    }
}