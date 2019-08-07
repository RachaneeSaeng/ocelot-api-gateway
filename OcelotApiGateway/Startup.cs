using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ocelot.DependencyInjection;
using Ocelot.Logging;
using Ocelot.Middleware;
using Ocelot.Provider.Polly;
using System;
using System.Net.Http;
using System.Reflection;

namespace OcelotApiGateway
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // set forward header keys to be the same value as request's header keys
            // so that redirect URIs and other security policies work correctly.
            if (string.Equals(Environment.GetEnvironmentVariable("ASPNETCORE_FORWARDEDHEADERS_ENABLED"), "true", StringComparison.OrdinalIgnoreCase))
            {
                //To forward the scheme from the proxy in non-IIS scenarios
                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                    // Only loopback proxies are allowed by default.
                    // Clear that restriction because forwarders are enabled by explicit 
                    // configuration.
                    options.KnownNetworks.Clear();
                    options.KnownProxies.Clear();
                });
            }

            services.TryAddSingleton<IOcelotLoggerFactory, AspDotNetLoggerFactory>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton<HttpClient, HttpClient>();
            services.TryAddSingleton<SwaggerLoader, SwaggerLoader>();
            services
                .AddOcelot()
                .AddPolly();
            //.AddAdministration("/ocelot", "abc123");
            //AddCacheManager
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseForwardedHeaders();
            //app.UseHsts();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseMvc();

            app.UseSwaggerUI(options =>
            {
                var configurationSection = Configuration.GetSection("DownstreamSwaggerUrls");
                foreach (var version in configurationSection.GetChildren())
                {
                    options.SwaggerEndpoint($"/swagger/{version.Key}/swagger.json", version.Key);
                }

                options.IndexStream = () => Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("OcelotApiGateway.wwwroot.swagger.ui.index.html");
                options.InjectBaseUrl(Configuration["GlobalConfiguration:BaseUrl"]);
            });

            app.UseOcelot().Wait(); // admin path won't work when call after UseMvc(). But If it's called before UseMvc(), swagger and customer endpoints won't work.
        }
    }
}
