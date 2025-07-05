namespace PainKiller.CommandPrompt.CoreLib.Core.DomainObjects;
/// <param name="TerminateProgram">
/// If set to true, the Command Prompt host will terminate after this command has executed.
/// Useful for scheduled or one-off command scenarios.
/// </param>
public record RunResult(string Identifier, bool Success, string Message,bool TerminateProgram = false);
