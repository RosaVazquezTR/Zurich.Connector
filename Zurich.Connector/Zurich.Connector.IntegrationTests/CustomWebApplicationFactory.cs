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
                    _configuration = new ConfigurationBuilder()
                      .AddJsonFile("integrationsettings.json")
                      .AddUserSecrets(Assembly.GetExecutingAssembly())
                        .Build();


                    configBuilder.AddAzureKeyVault(
                        _configuration["KeyVault:Endpoint"],
                        _configuration["AzureAd:ClientId"],
                        _configuration["AzureAd:ClientSecret"]);

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
