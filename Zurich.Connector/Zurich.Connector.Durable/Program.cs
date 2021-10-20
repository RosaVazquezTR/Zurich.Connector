using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Zurich.Common;
using Zurich.Common.Repositories;
using Zurich.Common.Services;
using Zurich.Common.Services.Security;
using Zurich.Connector.App;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Durable.Configuration;
using Zurich.Connector.Durable.Repository;
using Zurich.Connector.Durable.Service;
using Zurich.TenantData;

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
                            configurationOptions.TokenIssuer, s.GetRequiredService<ILogger<TokenAuthorityDiscoveryService>>(), s.GetRequiredService<IOIDCAuthorityRepo>(), 
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