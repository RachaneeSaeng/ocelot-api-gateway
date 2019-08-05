using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace OcelotApiGateway
{
    public class SwaggerLoader
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;

        public SwaggerLoader(HttpClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
        }

        public async Task<string> LoadSwagger(string version)
        {
            var configurationSection = _config.GetSection("DownstreamSwaggerUrls");

            var swaggerUrls = configurationSection[version]?.Split(',');

            if (swaggerUrls == null)
                return "No document for API version " + version;

            var rawSwaggerSpecs = await LoadSwaggerSpecs(swaggerUrls);
            var swaggerSpecs = rawSwaggerSpecs.Select(spec => spec.Replace("/api/", $"/{version}/api/"));

            var jObject = CreateJsonObject(swaggerSpecs);

            return jObject.ToString();
        }

        private async Task<List<string>> LoadSwaggerSpecs(string[] swaggerUrls)
        {
            var swaggerSpecs = new List<string>();
            foreach (var url in swaggerUrls)
            {
                var responseString = await _client.GetStringAsync(url);
                if (!string.IsNullOrEmpty(responseString))
                {
                    swaggerSpecs.Add(responseString);
                }
            }
            return swaggerSpecs;
        }

        private JObject CreateJsonObject(IEnumerable<string> swaggerSpecs)
        {
            var jObject = new JObject();

            foreach (var spec in swaggerSpecs)
            {
                var specObject = JObject.Parse(spec);

                jObject.Merge(
                    specObject,
                    new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
            }

            // Overide common spec
            var commonSpec = JObject.Parse(@"{
                                    'info': {
                                        'version': 'v1',
                                        'title': 'Central API'
                                    },
                                    'securityDefinitions': {
                                        'Bearer': {
                                            'type': 'basic'
                                        }
                                    }
                                }");

            jObject.Merge(
                    commonSpec,
                    new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });

            return jObject;
        }

    }
}
