namespace PainKiller.DockubeClient.Commands;

[CommandDesign(description:  "Dockube -  Run kubectl command",
                  examples: ["//Run kubectl to get nodes", "k get nodes"])]
public class KCommand(string identifier) : ProxyCommando<CommandPromptConfiguration>(identifier, alias:"kubectl") { }