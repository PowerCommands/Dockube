using DockubeApi.Configuration.Services;
using DockubeApi.Configuration.DomainObjects;
using DockubeApi.DomainObjects;
using DockubeApi.Services;


var builder = WebApplication.CreateBuilder(args);

// 🔧 Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenApi();
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// 🔧 Enable Swagger in dev
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
var configuration = ConfigurationService.Service.Get<DockubeApiConfiguration>().Configuration;
var gitlabService = new GitlabService(configuration.Gitlab.BaseUrl, configuration.Gitlab.AccessToken);

app.MapGet("/version", () => $"{configuration.Core.Name} {configuration.Core.Version}").WithName("Version");

app.MapPost("/gitlab/project", async (GitlabProjectRequest request) =>
{
    var result = await gitlabService.CreateProjectAsync(request.ProjectName);
    return Results.Ok(new { ProjectId = result });
})
.WithName("CreateGitlabProject")
.WithDescription("Creates a new GitLab project with the given name")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status500InternalServerError);

app.Run();