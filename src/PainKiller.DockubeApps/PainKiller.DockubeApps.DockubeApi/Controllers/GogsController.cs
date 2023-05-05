using Microsoft.AspNetCore.Mvc;

namespace PainKiller.DockubeApps.DockubeApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GogsController : ControllerBase
    {
        private readonly ILogger<GogsController> _logger;
        public GogsController(ILogger<GogsController> logger) => _logger = logger;

        [HttpGet(Name = "repos")]
        public IEnumerable<string> GetRepos()
        {
            var retVal = new List<string>();

            return retVal;
        }
    }
}