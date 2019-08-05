using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;

namespace OcelotApiGateway.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class SwaggerController : ControllerBase
    {
        [HttpGet]
        [Route("v1/swagger.json")]
        public async Task<string> GetV1()
        {
            var _client = new HttpClient();
            var responseString = await _client.GetStringAsync($"http://localhost:22742/swagger/v1/swagger.json");
            return responseString;
        }

        [HttpGet]
        [Route("v2/swagger.json")]
        public async Task<string> GetV2()
        {
            var _client = new HttpClient();
            var responseString = await _client.GetStringAsync($"https://localhost:44390/swagger/v1/swagger.json");
            return responseString;
        }
    }
}