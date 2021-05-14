using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zurich.Connector.Web
{
    public class Program
    {
        private static IConfigurationRefresher configurationRefresher;
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IConfigurationRefresher ConfigurationRefresher
        {
            get { return configurationRefresher; }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((HostBuilderContext context, IConfigurationBuilder configBuilder) =>
                {
                    var builtConfig = configBuilder.Build();

                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        configBuilder.AddAzureKeyVault(
                            builtConfig["KeyVault:Endpoint"],
                            builtConfig["AzureAd:ClientId"],
                            builtConfig["AzureAd:ClientSecret"]);
                    }
                    else
                    {
                        var azureServiceTokenProvider = new AzureServiceTokenProvider();
                        var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                        configBuilder.AddAzureKeyVault(builtConfig["KeyVault:Endpoint"], keyVaultClient, new DefaultKeyVaultSecretManager());
                    }

                    builtConfig = configBuilder.Build();

                    // TODO: May not be using an app config in this RG. Update appsettings files 
                    //var connection = builtConfig.GetValue<string>("AppConfig:ConnectionString");
                    //if (!string.IsNullOrEmpty(connection))
                    //{
                    //    configBuilder.AddAzureAppConfiguration(options =>
                    //    {
                    //        options.Connect(connection)
                    //            .Select(KeyFilter.Any, "Beta")
                    //            .UseFeatureFlags(featureOptions => {
                    //                featureOptions.CacheExpirationTime = TimeSpan.FromMinutes(5);
                    //                featureOptions.Label = "Beta";
                    //            });

                    //        configurationRefresher = options.GetRefresher();
                    //    });
                    //}
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    // Add Serilog?
                    webBuilder.UseStartup<Startup>();
                });
    }
}
