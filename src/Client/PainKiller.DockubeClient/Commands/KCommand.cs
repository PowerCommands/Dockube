namespace PainKiller.DockubeClient.Commands;
public class KCommand(string identifier) : ProxyCommando<CommandPromptConfiguration>(identifier, alias:"kubectl") { }