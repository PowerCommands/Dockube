namespace PainKiller.DockubeClient.Commands;
[CommandDesign(description:  "Dockube -  Run docker command",
                  examples: ["//Run docker command to view version", "docker --version"])]
public class DockerCommand(string identifier) : TerminalCommando<CommandPromptConfiguration>(identifier) { }

//TODO: logga in på en container instans
//docker exec -u 0 -it splunk bash 

//Admin kommandon körs så här
///opt/splunk/bin/splunk list input tcp -auth admin:YOUR_PASSWORD