using System.IO;
using Microsoft.AspNetCore.Mvc;

namespace Diagnosticos.Api.Controllers
{
    [ApiController]
    [Route("/")]
    public class DefaultController : ControllerBase
    {
        [HttpGet]
        public string Index()
        {
            var productionPrologPath = Path.GetFullPath($"./pl/enfermedad.pl");
            return productionPrologPath;
        }
    }
}
