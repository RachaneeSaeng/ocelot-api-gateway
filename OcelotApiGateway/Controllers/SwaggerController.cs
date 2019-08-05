using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace OcelotApiGateway.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SwaggerController : ControllerBase
    {
        private SwaggerLoader _swgLoader;
        public SwaggerController(SwaggerLoader swgLoader)
        {
            _swgLoader = swgLoader;
        }

        [HttpGet]
        [Route("{version}/swagger.json")]
        public async Task<string> Get(string version)
        {
            var swaggerJson = await _swgLoader.LoadSwagger(version);
            return swaggerJson;
        }

    }
}