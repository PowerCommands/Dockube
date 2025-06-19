using DockubeApi.Configuration.Services;
using DockubeApi.Configuration.DomainObjects;
using DockubeApi.DomainObjects;
using DockubeApi.Services;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

var configuration = ConfigurationService.Service.Get<DockubeApiConfiguration>().Configuration;

var certPath = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path") ?? throw new InvalidOperationException("Cert path not configured");
var certPassword = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password") ?? throw new InvalidOperationException("Cert password not configured");
var gitlabToken = Environment.GetEnvironmentVariable("DOCKUBE__GITLAB__TOKEN") ?? throw new InvalidOperationException("DOCKUBE__GITLAB__TOKEN not configured");

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ServerCertificate = X509CertificateLoader.LoadPkcs12FromFile(certPath, certPassword);
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

var gitlabService = new GitlabService(configuration.Gitlab.BaseUrl, gitlabToken);

app.MapGet("/version", () => $"{configuration.Core.Name} {configuration.Core.Version}").WithName("Version");

app.MapPost("/gitlab/project", async (GitlabProjectRequest request) => { var result = await gitlabService.CreateProjectAsync(request.ProjectName); return Results.Ok(new { ProjectId = result }); })
.WithName("CreateGitlabProject")
.WithDescription("Creates a new GitLab project with the given name")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status500InternalServerError);

app.MapPost("/gitlab/project/file", async (CreateOrUpdateFileRequest request) =>
{
    var result = await gitlabService.CreateOrUpdateFileAsync( request.ProjectId, request.FilePath, request.Branch, request.Content, request.CommitMessage );

    return result ? Results.Ok(new { Message = "File created or updated successfully." }) : Results.StatusCode(StatusCodes.Status500InternalServerError);
})
.WithName("CreateOrUpdateGitlabFile")
.WithDescription("Creates or updates a file in a GitLab project")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status500InternalServerError);

app.MapPost("/gitlab/project/file/delete", async (DeleteFileRequest request) =>
{
    var result = await gitlabService.DeleteFileAsync( request.ProjectId, request.FilePath, request.Branch, request.CommitMessage);

    return result ? Results.Ok(new { Message = "File created or updated successfully." }) : Results.StatusCode(StatusCodes.Status500InternalServerError);
})
.WithName("DeleteGitlabFile")
.WithDescription("Creates or updates a file in a GitLab project")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status500InternalServerError);

app.Run();