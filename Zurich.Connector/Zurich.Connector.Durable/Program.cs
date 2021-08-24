using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Configuration.AzureKeyVault;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Polly;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Common;
using Zurich.Common.Models;
using Zurich.Common.Models.Cosmos;
using Zurich.Common.Models.HighQ;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Repositories;
using Zurich.Common.Services;
using Zurich.Common.Services.Security;
using Zurich.TenantData;
using System.Net.Http;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Data.SqlClient.AlwaysEncrypted.AzureKeyVaultProvider;
using Microsoft.Data.SqlClient;
using Zurich.Connector.Durable.Configuration;
using Zurich.Connector.Durable.Repository;
using Zurich.Connector.Durable.Service;
using Zurich.Connector.App;
using Zurich.Connector.App.Services;

namespace Zurich.Connector.Durable
{
    public class Program
    {

        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                    ConfigurationLoader loader = new ConfigurationLoader();
                    var configurationOptions = loader.GetConfigurationProviders(services);
                    loader.ConfigureStoreEncryption(configurationOptions.AzureAdOptions);

                    services.AddLogging();
                    services.AddAutoMapper(typeof(CommonMappingsProfile), typeof(MappingRegistrar), typeof(ServiceMappingRegistrar));
                    services.AddDbContext<TenantContext>(options => options.UseSqlServer(configurationOptions.OAuthDbConnString), ServiceLifetime.Transient);
                    services.AddPartnerAppAuth(configurationOptions.OAuthOptions, configurationOptions.MicroServOptions);
                    services.AddSingleton<IOIDCAuthorityRepo, OIDCAuthorityRepo>();
                    services.AddSingleton<ITokenAuthorityDiscoveryService, TokenAuthorityDiscoveryService>(s => new TokenAuthorityDiscoveryService(
                            configurationOptions.Issuer, s.GetRequiredService<ILogger<TokenAuthorityDiscoveryService>>(), s.GetRequiredService<IOIDCAuthorityRepo>(), 
                            s.GetRequiredService<System.Net.Http.IHttpClientFactory>()));

                    services.AddSingleton<ITokenAuthenticationService, FunctionsTokenAuthenticationService>(s => new FunctionsTokenAuthenticationService(
                            configurationOptions.Audience, s.GetRequiredService<ILogger<FunctionsTokenAuthenticationService>>(), s.GetRequiredService<ITokenAuthorityDiscoveryService>())
                    );
                    services.AddConnectorCosmosServices(configurationOptions.ConnectorCosmosDbOptions, configurationOptions.ConnectorCosmosClientOptions);
                    services.AddScoped<ISessionAccessor, FunctionsSessionAccessor>();
                    services.AddHttpClient<IPracticalLawRepository, PracticalLawRepository>();
                    services.AddScoped<IFilterService, FilterService>();
                    services.AddScoped<IPLService, PLService>();
                    services.AddScoped<ICosmosService, CosmosService>();
                })
                .Build();

            host.Run();
        }
    }
}