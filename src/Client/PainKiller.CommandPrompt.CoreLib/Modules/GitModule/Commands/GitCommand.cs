using PainKiller.CommandPrompt.CoreLib.Core.BaseClasses;
using PainKiller.CommandPrompt.CoreLib.Core.Extensions;
using PainKiller.CommandPrompt.CoreLib.Metadata.Attributes;
using PainKiller.CommandPrompt.CoreLib.Modules.ShellModule.Services;

namespace PainKiller.CommandPrompt.CoreLib.Modules.GitModule.Commands;

[CommandDesign(
    description: "Example shows how to execute an external program (git) to commit/push/status/log/branch a repository configured in the configuration file.",
    arguments: ["commit", "push", "status", "log", "branch"],
    options: ["create", "change", "delete", "merge", "main", "relative-path"],
    quotes: ["\"<comment>\" defaults to \"refactoring\" if omitted, only used with commit."],
    suggestions: ["commit", "push", "status", "log", "branch"],
    examples:
    [
        "//Add and commit",
        "git commit \"Bugfix\"",
        "//Performs a push to Git repo",
        "git push",
        "//Git status of the configured git repo",
        "git status",
        "//Show log",
        "git log",
        "//Create and change to branch",
        "git branch --create my-branch",
        "//Change branch",
        "git branch --change my-branch",
        "//Merge branch",
        "git branch --merge my-branch",
        "//Delete branch locally (and remote if you want)",
        "git branch --delete my-branch",
        "//Change to main branch",
        "git branch --main"
    ]
)]
public class GitCommand(string identifier) : ConsoleCommandBase<ApplicationConfiguration>(identifier)
{
    public override RunResult Run(ICommandLineInput input)
    {
        if (input.HasOption("relative-path"))
        {
            var relativePath = GetGitRelativePath();
            Writer.WriteDescription("Relative path", relativePath);
            return Ok();
        }

        var action = input.Arguments.FirstOrDefault()?.ToLowerInvariant();
        var singleQuote = input.Quotes.FirstOrDefault() ?? string.Empty;

        switch (action)
        {
            case "commit":
                Commit(singleQuote);
                break;

            case "branch":
                HandleBranch(input);
                break;

            case "merge":
                Merge(singleQuote);
                break;

            case "push":
            case "status":
            case "log":
                RunSingleCommand(action, singleQuote);
                break;

            default:
                return Nok($"Parameter '{action}' not supported. Try: status|commit|push|log|branch");
        }

        return Ok();
    }

    private void HandleBranch(ICommandLineInput input)
    {
        if (input.HasOption("change"))
            RunSingleCommand("checkout", input.GetOptionValue("change"));

        if (input.HasOption("create"))
            Create(input.GetOptionValue("create"));

        if (input.HasOption("main"))
            RunSingleCommand("checkout", "main");

        if (input.HasOption("merge"))
            Merge(input.GetOptionValue("merge"));

        if (input.HasOption("delete"))
            Delete(input.GetOptionValue("delete"));
    }

    private void Commit(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            comment = "refactoring";

        // Stage all
        RunSingleCommand("add", ".");

        // Commit
        ExecGit($"commit -m \"{comment}\"");
        Writer.WriteLine($"[GIT] commit -m \"{comment}\"");
    }

    private void RunSingleCommand(string command, string? name = null)
    {
        var arg = string.IsNullOrWhiteSpace(name) ? "" : $" {name}";
        Writer.WriteHeadLine($"Local repo path: {Configuration.Core.Modules.Git.DefaultRepositoryPath}\n");
        ExecGit($"{command}{arg}");
        Writer.WriteLine($"[GIT] {command}{arg}");
    }

    private void Merge(string branchName)
    {
        if (string.IsNullOrWhiteSpace(branchName))
        {
            Writer.WriteWarning("Branch name is missing.", scope: nameof(GitCommand));
            return;
        }

        RunSingleCommand("checkout", "main");
        ExecGit($"merge \"{branchName}\"");
        Writer.WriteLine($"[GIT] merge \"{branchName}\"");
    }

    private void Delete(string branchName)
    {
        if (string.IsNullOrWhiteSpace(branchName))
        {
            Writer.WriteWarning("Branch name is missing.", scope: nameof(GitCommand));
            return;
        }

        RunSingleCommand("checkout", "main");

        // Locally
        ExecGit($"branch \"{branchName}\" -D");
        Writer.WriteLine($"[GIT] branch \"{branchName}\" -D");

        var deleteRemote = DialogService.YesNoDialog("Do you also want to delete the branch remote (on the server)?");
        if (!deleteRemote) return;

        // Remote (server)
        ExecGit($"push origin --delete \"{branchName}\"");
        Writer.WriteLine($"[GIT] push origin --delete \"{branchName}\"");
    }

    private void Create(string branchName)
    {
        if (string.IsNullOrWhiteSpace(branchName))
        {
            Writer.WriteWarning("Branch name is missing.", scope: nameof(GitCommand));
            return;
        }

        RunSingleCommand("branch", branchName);

        ExecGit($"checkout \"{branchName}\"");
        Writer.WriteLine($"[GIT] checkout \"{branchName}\"");

        ExecGit($"push --set-upstream origin \"{branchName}\"");
        Writer.WriteLine($"[GIT] push --set-upstream origin \"{branchName}\"");
    }
    private void ExecGit(string arguments) => ShellService.Default.Execute("git", arguments, workingDirectory: Configuration.Core.Modules.Git.DefaultRepositoryPath, waitForExit: true);

    private static string GetGitRelativePath()
    {
        var path = AppContext.BaseDirectory;
        var relativePath = @"..\";
        var gitFound = false;

        const int maxRepeatCount = 15;
        var iterationCount = 0;

        while (!gitFound)
        {
            iterationCount++;

            var skipLast = path.EndsWith("\\", StringComparison.Ordinal) ? 2 : 1;
            var parts = path.Split(Path.DirectorySeparatorChar).SkipLast(skipLast);
            path = string.Join(Path.DirectorySeparatorChar, parts);

            var directory = new DirectoryInfo(path);
            gitFound = directory.GetDirectories().Any(d => d.Name.StartsWith(".git", StringComparison.OrdinalIgnoreCase));

            if (!gitFound) relativePath += @"..\";
            if (iterationCount > maxRepeatCount) break;
        }

        return relativePath;
    }
}