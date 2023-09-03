using Microsoft.AspNetCore.Mvc;

namespace Dockube.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("Dockube api up and running...");
}