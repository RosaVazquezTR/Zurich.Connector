using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Durable.Model;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Zurich.Common.Models.Cosmos;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Cosmos;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Models.HighQ;
using Zurich.Common.Models;
using Microsoft.Data.SqlClient.AlwaysEncrypted.AzureKeyVaultProvider;
using Microsoft.Data.SqlClient;

namespace Zurich.Connector.Durable.Configuration
{
    /// <summary>
    /// Configuration Loader
    /// </summary>
    public class ConfigurationLoader
    {
        string configurationBasePath = $"{Environment.GetEnvironmentVariable("HOME")}/site/wwwroot";
        private ClientCredential _clientCredential;

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
                            featureOptions.CacheExpirationTime = TimeSpan.FromMinutes(5);
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
            loaderOptions.OAuthDbConnString = configuration.GetValue<string>("EncryptedDBConnectionString");

            //TODO Probably get rid of this at some point
            if (env.Equals("Development", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(configuration.GetValue<string>("LocalEncryptedDBConnectionString")))
            {
                loaderOptions.OAuthDbConnString = configuration.GetValue<string>("LocalEncryptedDBConnectionString");
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
            _clientCredential = new ClientCredential(azureAdOptions.ClientId, azureAdOptions.ClientSecret);

            SqlColumnEncryptionAzureKeyVaultProvider azureKeyVaultProvider = new SqlColumnEncryptionAzureKeyVaultProvider(GetEncryptionKeyVaultToken);
            var providers = new Dictionary<string, SqlColumnEncryptionKeyStoreProvider>();
            providers.Add(SqlColumnEncryptionAzureKeyVaultProvider.ProviderName, azureKeyVaultProvider);
            SqlConnection.RegisterColumnEncryptionKeyStoreProviders(providers);

        }

        /// <summary>
        /// Requests an application token for acessing the column encryption key in an Azure keyvault
        /// </summary>
        /// <param name="authority">The authority requesting the token</param>
        /// <param name="resource">The requested resource</param>
        /// <param name="scope">The requested scope</param>
        /// <returns></returns>
        public async Task<string> GetEncryptionKeyVaultToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, _clientCredential);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain access token");

            return result.AccessToken;
        }

    }

}
