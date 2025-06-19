public class CreateOrUpdateFileRequest
{
    public string ProjectId { get; set; } = default!;
    public string FilePath { get; set; } = default!;
    public string Branch { get; set; } = "main";
    public string Content { get; set; } = default!;
    public string CommitMessage { get; set; } = "Update file";
}