﻿using Microsoft.Extensions.Configuration;
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
            SetCommonInfo(jObject, version);

            return jObject.ToString();
        }

        private async Task<List<string>> LoadSwaggerSpecs(string[] swaggerUrls)
        {
            var swaggerSpecs = new List<string>();
            foreach (var url in swaggerUrls)
            {
                try
                {
                    var responseString = await _client.GetStringAsync(url);
                    if (!string.IsNullOrEmpty(responseString))
                    {
                        swaggerSpecs.Add(responseString);
                    }
                }
                catch (HttpRequestException)
                {
                    continue;
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

            return jObject;
        }

        private void SetCommonInfo(JObject jObject, string version)
        {
            // Overide common spec
            var info = @"{
    'swagger': '2.0',
    'info': {
        'version': '{0}',
        'title': 'Central API'
    },
    'securityDefinitions': {
        'Bearer': {
            'type': 'basic'
        }
    }
}";
            info = info.Replace("'{0}'", $"'{version}'");
            var commonSpec = JObject.Parse(info);

            jObject.Merge(
                    commonSpec,
                    new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Union });
        }
    }
}
