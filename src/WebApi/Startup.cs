using System;
using System.IO;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;

namespace WebApi
{
    public class Startup
    {
        private bool _useSwagger;
        private const string ApiName = "tracker-webapi";
        private string ApiVersion;
        private const string codeDocumentation = "WebApi.xml";

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;

            _useSwagger = Configuration.GetValue<bool>("UseSwagger", env.IsDevelopment());
            Console.WriteLine($"Using Swagger Interface: {_useSwagger}");

            Console.WriteLine($"Role: {ApiName}.");
            ApiVersion = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            Console.WriteLine($"Version: {ApiVersion}.");
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<ITelemetryInitializer>(new Infrastructure.ApplicationInsights.Initializers.CloudRoleName(ApiName));

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddHealthChecks();

            if (_useSwagger)
            {
                // Register the Swagger generator, defining one or more Swagger documents
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info { Title = ApiName, Version = $"V{ApiVersion}" });

                });

                services.ConfigureSwaggerGen(options =>
                {
                    //Set the comments path for the swagger json and ui.
                    var basePath = PlatformServices.Default.Application.ApplicationBasePath;
                    var xmlPath = Path.Combine(basePath, codeDocumentation);

                    if (File.Exists(xmlPath))
                    {
                        options.IncludeXmlComments(xmlPath);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("File {0} not found", xmlPath));
                    }
                });

            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseHealthChecks("/health");
            app.UseMvc();


            if (_useSwagger)
            {
                // Enable middleware to serve generated Swagger as a JSON endpoint.
                app.UseSwagger();

                // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint.
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("v1/swagger.json", $"{ApiName} V{ApiVersion}");
                });
            }
        }
    }
}
