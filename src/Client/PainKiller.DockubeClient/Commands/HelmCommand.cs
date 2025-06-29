namespace PainKiller.DockubeClient.Commands;

[CommandDesign(description:  "Dockube -  Run helm command",
                  examples: ["//Run helm to get version", "helm version"])]
public class HelmCommand(string identifier) : ProxyCommando<CommandPromptConfiguration>(identifier) { }