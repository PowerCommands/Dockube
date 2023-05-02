﻿using DockubeCommands.Contracts;
using DockubeCommands.DomainObjects;
using System.Text;

namespace DockubeCommands.Managers;

public class PublishManager : IPublishManager
{
    private readonly string _workingDirectory;
    private string _lastReadLine = "";

    public PublishManager(string workingDirectory) => _workingDirectory = workingDirectory;
    public void Publish(string path, string kubernetesNamespace)
    {
        var yamlFiles = Directory.GetFiles(path).OrderBy(f => f).ToList();
        foreach (var fileName in yamlFiles)
        {
            if(fileName.ToLower().EndsWith(".yaml")) ApplyYamlFile(kubernetesNamespace, fileName);
            if(fileName.ToLower().EndsWith(".json")) HandleJsonFile(fileName);
        }
        if (!string.IsNullOrEmpty(kubernetesNamespace)) ShellService.Service.Execute("kubectl",$"config set-context --current --namespace={kubernetesNamespace}","", WriteLine,"", waitForExit: true);
    }

    private void HandleJsonFile(string fileName)
    {
        var fileInfo = new FileInfo(fileName);
        var processMetadata = StorageService<ProcessMetadata>.Service.GetObject(fileName);
        if (processMetadata.WaitSec > 0)
        {
            for (int i = 0; i < processMetadata.WaitSec; i++)
            {
                OverwritePreviousLine($"Waiting a {processMetadata.WaitSec-i} seconds, before executing [{processMetadata.Description}] ...");
                Thread.Sleep(1000);
            }
        }
        WriteLine(processMetadata.Description);
        WriteSuccessLine($"{fileInfo.Name} executed OK");
        if (string.IsNullOrEmpty(processMetadata.Url))
        {
            var applicationName = processMetadata.Name.Replace(".exe", "").Replace(".bat", "").Replace(".cmd", "");
            if (processMetadata.UseReadline)
            {
                ShellService.Service.Execute(applicationName, processMetadata.Args, _workingDirectory, ReadLine, "", waitForExit: processMetadata.WaitForExit, useShellExecute: processMetadata.UseShellExecute, disableOutputLogging: processMetadata.DisableOutputLogging);
                var token = processMetadata.Base64Decode ?  Encoding.UTF8.GetString(Convert.FromBase64String(_lastReadLine)) : _lastReadLine;
                Console.WriteLine(token);
            }
            else
            {
                var replacePlaceHolderArgs = processMetadata.Args.Replace("%LAST_READ_LINE%", _lastReadLine);
                ShellService.Service.Execute(applicationName, replacePlaceHolderArgs, _workingDirectory, WriteLine, "", waitForExit: processMetadata.WaitForExit, useShellExecute: processMetadata.UseShellExecute, disableOutputLogging: processMetadata.DisableOutputLogging);
            }
        }
        else
        {
            var url = processMetadata.Url.Replace(".exe", "").Replace(".bat", "").Replace(".cmd", "");
            ShellService.Service.OpenWithDefaultProgram(url);
        }
    }
    private void ApplyYamlFile(string nspace, string fileName)
    {
        var nmnSpace = nspace;
        if (!string.IsNullOrEmpty(nmnSpace) && !fileName.ToLower().Contains("namespace")) nmnSpace = $"-n {nspace}";
        else nmnSpace = "";
        var fileInfo = new FileInfo(fileName);
        ShellService.Service.Execute("kubectl", $"apply {nmnSpace} -f {fileInfo.FullName}", _workingDirectory, WriteLine, "", waitForExit: true);
        WriteSuccessLine($"{fileInfo.Name} applied OK");
    }

    #region Write helpers
    protected void ReadLine(string output) => _lastReadLine = output;
    protected void OverwritePreviousLine(string output)
    {
        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
        var padRight = Console.BufferWidth - output.Length;
        WriteLine(output.PadRight(padRight > 0 ? padRight : 0));
    }
    protected void WriteLine(string output) => ConsoleService.Service.WriteLine(nameof(PublishManager), writeLog: true, text:output);
    protected void WriteSuccessLine(string output) => ConsoleService.Service.WriteSuccessLine(nameof(PublishManager), text:output, writeLog: true);
    #endregion
}