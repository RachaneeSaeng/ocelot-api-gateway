using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace OcelotApiGateway.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private readonly ILogger<ValuesController> _logger;
        //private readonly IOcelotLogger _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;

        public ValuesController(
            ILogger<ValuesController> logger,
        //IOcelotLoggerFactory loggerFactory, 
        IHttpContextAccessor httpContextAccessor,
        IConfiguration config
        )
        {
            _logger = logger;
            //_logger = loggerFactory.CreateLogger<ValuesController>();
            _httpContextAccessor = httpContextAccessor;
            _configuration = config;
        }

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            //var cfg = _configuration["GGGG"];
            //var x = Get("");
            //_logger.LogInformation($"-*-*-*-*-*- {x}");
            _logger.LogInformation("Index page says hello");
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }

        public string Get(string key)
        {
            return _httpContextAccessor.HttpContext.TraceIdentifier;
            //object obj;

            //if (_httpContextAccessor.HttpContext == null || _httpContextAccessor.HttpContext.Items == null)
            //{
            //    return $"Unable to find data for key: {key} because HttpContext or HttpContext.Items is null";
            //}

            //if (_httpContextAccessor.HttpContext.Items.TryGetValue(key, out obj))
            //{
            //    var data = (string)obj;
            //    return data;
            //}

            //return $"Unable to find data for key: {key}";
        }
    }
}
