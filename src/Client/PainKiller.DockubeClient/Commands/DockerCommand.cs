namespace PainKiller.DockubeClient.Commands;
[CommandDesign(description:  "Dockube -  Run docker command",
                  examples: ["//Run docker command to view version", "docker --version"])]
public class DockerCommand(string identifier) : TerminalCommando<CommandPromptConfiguration>(identifier) { }