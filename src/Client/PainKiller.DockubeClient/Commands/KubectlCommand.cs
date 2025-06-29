namespace PainKiller.DockubeClient.Commands;

[CommandDesign(description:  "Dockube -  Run kubectl command",
                  examples: ["//Run kubectl to get nodes", "kubectl get nodes"])]
public class KubectlCommand(string identifier) : ProxyCommando<CommandPromptConfiguration>(identifier) { }