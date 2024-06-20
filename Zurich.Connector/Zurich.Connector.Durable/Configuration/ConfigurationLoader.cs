using Azure.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.AlwaysEncrypted.AzureKeyVaultProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using Zurich.Common.Models;
using Zurich.Common.Models.Cosmos;
using Zurich.Common.Models.HighQ;
using Zurich.Common.Models.OAuth;
using Zurich.Connector.Durable.Model;

namespace Zurich.Connector.Durable.Configuration
{
    /// <summary>
    /// Configuration Loader
    /// </summary>
    public class ConfigurationLoader
    {
        string configurationBasePath = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";

        /// <summary>
        /// Setup configuration providers.
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public ConfiguratinoLoaderOptions GetConfigurationProviders(IServiceCollection services)
        {
            // If local environment
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_INSTANCE_ID")))
            {
                configurationBasePath = Directory.GetCurrentDirectory();
            }

            ConfiguratinoLoaderOptions loaderOptions = new ConfiguratinoLoaderOptions();

            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            string json = "appsettings.json";
            string envJson = $"appsettings.{env}.json";

            IConfigurationBuilder builder = new ConfigurationBuilder()
                .SetBasePath(configurationBasePath)
                .AddJsonFile(json, optional: true)
                .AddJsonFile(envJson, optional: true)
                .AddEnvironmentVariables();

            IConfiguration configuration = builder.Build();

            if (env != null && env.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                builder.AddAzureKeyVault(
                    configuration["KeyVault:Endpoint"],
                    configuration["AzureAd:ClientId"],
                    configuration["AzureAd:ClientSecret"]);
            }
            else
            {
                var azureServiceTokenProvider = new AzureServiceTokenProvider();
                var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                builder.AddAzureKeyVault(configuration["KeyVault:Endpoint"], keyVaultClient, new DefaultKeyVaultSecretManager());
            }

            configuration = builder.Build();

            var connectionString = configuration.GetValue<string>("AppConfig:ConnectionString");

            if (!string.IsNullOrEmpty(connectionString))
            {
                builder.AddAzureAppConfiguration(options =>
                {
                    options.Connect(connectionString)
                        .Select(KeyFilter.Any, configuration["LabelFilter"])
                        .UseFeatureFlags(featureOptions =>
                        {
                            // TODO: Make CacheExpirationTime and possibly Label values dynamic and add a variable to a global file somewhere 
                            featureOptions.CacheExpirationInterval = TimeSpan.FromMinutes(5);
                            featureOptions.Label = configuration["LabelFilter"];
                        });

                    loaderOptions.ConfigurationRefresher = options.GetRefresher();
                });
            }

            configuration = builder.Build();
            SetupCosmos(services, configuration);
            loaderOptions.TokenIssuer = configuration.GetValue<string>("TokenIssuer");

            loaderOptions.OAuthOptions = new OAuthOptions();
            configuration.Bind("PartnerOAuthApps", loaderOptions.OAuthOptions);

            loaderOptions.MicroServOptions = new MicroServiceOptions();
            configuration.Bind("HighqMS", loaderOptions.MicroServOptions);

            loaderOptions.AzureAdOptions = new AzureAdOptions();
            configuration.Bind("AzureAd", loaderOptions.AzureAdOptions);

            loaderOptions.TableStorageOptions = new StatsStorageContainerOptions();
            configuration.Bind("StatsStorage", loaderOptions.TableStorageOptions);

            // TODO - This should be removed when we get the OAuthService setup.
            loaderOptions.OAuthDbConnString = configuration.GetConnectionString("LegalHomeDB");

            //TODO Probably get rid of this at some point
            if (env.Equals("Development", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(configuration.GetConnectionString("LocalLegalHomeDB")))
            {
                loaderOptions.OAuthDbConnString = configuration.GetConnectionString("LocalLegalHomeDB");
            }

            loaderOptions.ConnectorCosmosDbOptions = new CosmosDbOptions();
            configuration.Bind("CosmosDb", loaderOptions.ConnectorCosmosDbOptions);

            loaderOptions.ConnectorCosmosClientOptions = new CosmosClientSettings();
            configuration.Bind("Connector:CosmosClientOptions", loaderOptions.ConnectorCosmosClientOptions);

            loaderOptions.ConfigurationProviders = builder.Build().Providers;
            return loaderOptions;
        }

        private static void SetupCosmos(IServiceCollection services, IConfiguration configuration)
        {
            var cosmosDbOptions = new CosmosDbOptions();
            configuration.Bind("CosmosDb", cosmosDbOptions);

            var documentClient = new DocumentClient(new Uri(cosmosDbOptions.Endpoint), cosmosDbOptions.PrimaryKey);
            services.AddSingleton(cosmosDbOptions);
            services.AddSingleton(documentClient);

            var cosmosClientOptions = new CosmosClientSettings();
            configuration.Bind("CosmosClientOptions", cosmosClientOptions);
            services.AddSingleton(cosmosClientOptions);

            CosmosClientOptions options = new CosmosClientOptions()
            {
                AllowBulkExecution = cosmosClientOptions.AllowBulkExecution,
                ConnectionMode = Microsoft.Azure.Cosmos.ConnectionMode.Gateway,
                GatewayModeMaxConnectionLimit = cosmosClientOptions.GatewayModeMaxConnectionLimit == 0 ? 10 : cosmosClientOptions.GatewayModeMaxConnectionLimit,
                MaxRetryAttemptsOnRateLimitedRequests = cosmosClientOptions.MaxRetryAttemptsOnRateLimitedRequests == 0 ? 9 : cosmosClientOptions.MaxRetryAttemptsOnRateLimitedRequests,
                MaxRetryWaitTimeOnRateLimitedRequests = cosmosClientOptions.MaxRetryWaitTimeOnRateLimitedRequests == 0 ? new TimeSpan(0, 0, 30) : new TimeSpan(0, 0, cosmosClientOptions.MaxRetryWaitTimeOnRateLimitedRequests)
            };

            CosmosClient cosmosclient = new CosmosClient(cosmosDbOptions.Endpoint, cosmosDbOptions.PrimaryKey, options);
            services.AddSingleton(cosmosclient);
        }

        /// <summary>
        /// Sets up access to always encrypted columns in a SQL database
        /// </summary>
        public void ConfigureStoreEncryption(AzureAdOptions azureAdOptions)
        {
            var clientSecretCredential = new ClientSecretCredential(azureAdOptions.ApplicationTenant, azureAdOptions.ClientId, azureAdOptions.ClientSecret);

            SqlColumnEncryptionAzureKeyVaultProvider azureKeyVaultProvider = new(clientSecretCredential);
            var providers = new Dictionary<string, SqlColumnEncryptionKeyStoreProvider>
            {
                { SqlColumnEncryptionAzureKeyVaultProvider.ProviderName, azureKeyVaultProvider }
            };

            SqlConnection.RegisterColumnEncryptionKeyStoreProviders(providers);
        }
    }
}
