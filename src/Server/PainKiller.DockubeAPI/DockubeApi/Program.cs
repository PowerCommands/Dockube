using DockubeApi.Configuration.Services;
using PainKiller.DockubeClient.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
var app = builder.Build();

if (app.Environment.IsDevelopment()) app.MapOpenApi();
app.UseHttpsRedirection();
var configuration = ConfigurationService.Service.Get<DockubeApiConfiguration>().Configuration;

app.MapGet("/version", () =>
{   
    return $"{configuration.Core.Name} {configuration.Core.Version}";
})
.WithName("Version");

app.Run();