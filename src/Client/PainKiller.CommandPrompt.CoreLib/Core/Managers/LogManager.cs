using System.Text.RegularExpressions;

namespace PainKiller.CommandPrompt.CoreLib.Core.Managers;

public class LogManager : ILogManager
{
    public LogManager(string rootPath, string fileName)
    {
        RootPath = Path.Combine(AppContext.BaseDirectory, rootPath);
        var logFilePrefix = Path.GetFileNameWithoutExtension(fileName);
        CurrentFilePath = $"{Directory.GetFiles(RootPath, $"{logFilePrefix}*.log").OrderByDescending(File.GetLastWriteTime).FirstOrDefault()}";
    }
    public string RootPath { get; }
    public string CurrentFilePath { get; }
    public IEnumerable<LogEntry> GetLog()
    {
        var lines = SafeReadLines(CurrentFilePath);
        var logEntries = lines.Select(ParseLogLine).Select(parsed => new LogEntry { Timestamp = parsed.Timestamp, Level = parsed.Level, Message = parsed.Message }).ToList();
        logEntries.Reverse();
        return logEntries;
    }
    private (string Timestamp, string Level, string Message) ParseLogLine(string line)
    {
        var match = Regex.Match(line, @"^\[(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}) (\w+)\]\s+(.+)$");
        return !match.Success ? ("-", "-", line) : (match.Groups[1].Value, match.Groups[2].Value, match.Groups[3].Value);
    }
    private List<string> SafeReadLines(string path)
    {
        var lines = new List<string>();
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream) lines.Add(reader.ReadLine()!);
        return lines;
    }
}