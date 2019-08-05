using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OcelotApiGateway
{
    public class SwaggerHandler : DelegatingHandler
    {
        private readonly HttpClient _client;
        private readonly IConfiguration _config;
        public SwaggerHandler(HttpClient client, IConfiguration config)
        {
            _client = client;
            _config = config;
        }
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var swaggerUrls = _config["DownstreamSwaggerUrls"].Split(',');
            string json = "";

            foreach (var url in swaggerUrls)
            {
                var responseString = await _client.GetStringAsync(url);

                json = MergeJson(json, responseString);
            }

            // override swagger doc for some path
            var overrideContent = @"{
                                        'info': {
                                            'version': 'v1',
                                            'title': 'Central API'
                                        },
                                        'securityDefinitions': {
                                            'Bearer': {
                                                'type': 'basic'
                                            }
                                        }
                                    }";
            json = MergeJson(json, overrideContent);

            return new HttpResponseMessage() { Content = new StringContent(json) };
        }

        private string MergeJson(string json1, string json2)
        {
            if (string.IsNullOrWhiteSpace(json1))
            {
                return json2;
            }
            if (string.IsNullOrWhiteSpace(json2))
            {
                return json1;
            }

            JObject jsonObject1 = JObject.Parse(json1);
            JObject jsonObject2 = JObject.Parse(json2);

            jsonObject1.Merge(jsonObject2, new JsonMergeSettings
            {
                // union array values together to avoid duplicates
                MergeArrayHandling = MergeArrayHandling.Union
            });

            return jsonObject1.ToString();
        }
    }
}
