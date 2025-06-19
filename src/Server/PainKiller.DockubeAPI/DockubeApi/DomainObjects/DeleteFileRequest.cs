namespace DockubeApi.DomainObjects;

public class DeleteFileRequest
{
    public string ProjectId { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public string Branch { get; set; } = "main";    
    public string CommitMessage { get; set; } = "Update file";
}