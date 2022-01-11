using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Zurich.Connector.Web;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Zurich.Connector.Web.Configuration;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Zurich.Connector.IntegrationTests
{
    #region snippet1
    public class CustomWebApplicationFactory : WebApplicationFactory<Startup>
    {
        public IConfiguration _configuration { get; private set; }

        // Uses the generic host
        protected override IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((HostBuilderContext context, IConfigurationBuilder configBuilder) =>
                {
                string json = "integrationsettings.json";

                configBuilder = configBuilder.AddJsonFile(json, optional: true);

                _configuration = configBuilder.Build();

                string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? _configuration["ASPNETCORE_ENVIRONMENT"] ?? "Development";

                string envJson = $"integrationsettings.{env}.json";

                // TODO: arguably we should be able to use the same builder as above but the tests seem to break when running on hte build machine
                _configuration = new ConfigurationBuilder()
                    .AddJsonFile(json, optional: true)
                    .AddJsonFile(envJson, optional: true)
                    .AddUserSecrets(Assembly.GetExecutingAssembly())
                    .Build();

                configBuilder.AddAzureKeyVault(
                    _configuration["KeyVault:Endpoint"],
                    _configuration["AzureAd:ClientId"],
                    _configuration["AzureAd:ClientSecret"])
                    .AddJsonFile(envJson, optional: true);

                    _configuration = configBuilder.Build();

                });

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseStartup<FakeStartup>();

            builder.ConfigureTestServices(services =>
            {
               
            });
        }
    }

    #endregion
}
