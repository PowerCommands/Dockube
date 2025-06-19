using DockubeApi.Configuration.Services;
using DockubeApi.Configuration.DomainObjects;
using DockubeApi.DomainObjects;
using DockubeApi.Services;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// 🔧 Load config
var configuration = ConfigurationService.Service.Get<DockubeApiConfiguration>().Configuration;

// 🔐 Set up HTTPS 
var certPath = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Path") ?? throw new InvalidOperationException("Cert path not configured");
var certPassword = Environment.GetEnvironmentVariable("ASPNETCORE_Kestrel__Certificates__Default__Password") ?? throw new InvalidOperationException("Cert password not configured");

builder.WebHost.ConfigureKestrel(options =>
{
    options.ConfigureHttpsDefaults(httpsOptions =>
    {
        httpsOptions.ServerCertificate = new X509Certificate2(certPath, certPassword);
    });
});

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

// 🔧 GitLab Service
var gitlabService = new GitlabService(configuration.Gitlab.BaseUrl, configuration.Gitlab.AccessToken);

app.MapGet("/version", () => $"{configuration.Core.Name} {configuration.Core.Version}")
    .WithName("Version");

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