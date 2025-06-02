using System.Runtime.CompilerServices;
using Spectre.Console;
namespace PainKiller.CommandPrompt.CoreLib.Core.Contracts;
public interface IConsoleWriter
{
    void WriteDescription(string label, string text, string title = "Description", bool writeToLog = true, Color? consoleColor = null, bool noBorder = false, [CallerMemberName] string scope = "");
    void Write(string text, bool writeLog = true, Color? consoleColor = null ,[CallerMemberName] string scope = "");
    void WriteLine(string text = "", bool writeLog = true, Color? consoleColor = null, [CallerMemberName] string scope = "");
    void WriteSuccessLine(string text, bool writeLog = true, [CallerMemberName] string scope = "");
    void WriteWarning(string text, string scope);
    void WriteError(string text, string scope);
    void WriteCritical(string text, string scope);
    void WriteHeadLine(string text, bool writeLog = true, [CallerMemberName] string scope = "");
    void WriteUrl(string text, bool writeLog = true, [CallerMemberName] string scope = "");
    void WriteSeparator(string separator = "-");
    void WriteTable<T>(IEnumerable<T> items, string[]? columnNames = null, Color? consoleColor = null, Color? borderColor = null, bool expand = true);
    void WritePrompt(string prompt);
    void WriteRowWithColor(int top, ConsoleColor foregroundColor, ConsoleColor backgroundColor, string rowContent);
    void Clear();
    void ClearRow(int top);
    void SetMargin(int reservedLines);
}