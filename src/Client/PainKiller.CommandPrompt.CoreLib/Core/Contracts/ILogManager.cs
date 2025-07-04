namespace PainKiller.CommandPrompt.CoreLib.Core.Contracts;

public interface ILogManager
{
    string RootPath { get; }
    string CurrentFilePath { get; }
    IEnumerable<LogEntry> GetLog();
}