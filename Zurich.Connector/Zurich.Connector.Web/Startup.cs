using Azure.Identity;
using IdentityModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.AlwaysEncrypted.AzureKeyVaultProvider;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Zurich.Common;
using Zurich.Common.Models;
using Zurich.Common.Models.Cosmos;
using Zurich.Common.Models.HighQ;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Services.Security;
using Zurich.Common.Services.Security.CIAM;
using Zurich.Connector.App;
using Zurich.Connector.App.Services;
using Zurich.Connector.App.Services.DataSources;
using Zurich.Connector.Data.Configuration;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Web.Configuration;
using Zurich.Connector.Web.Extensions;

namespace Zurich.Connector.Web
{
    public class Startup
    {
        private AzureAdOptions _azureOptions;
        private static OAuthOptions _oAuthOptions;
        private static MicroServiceOptions _microServOptions;
        private CosmosDbOptions _connectorCosmosDbOptions;
        private CosmosClientSettings _connectorCosmosClientOptions;
        private AuthIssuerOptions _legalPlatformAuthOptions;
        private CIAMAuthOptions _ciamAuthOptions;

        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public virtual void ConfigureServices(IServiceCollection services)
        {
            BindConfigurationOptions();

            string tenantConnectionString = Configuration.GetConnectionString("LegalHomeDB");
            if (Environment.IsDevelopment() && !string.IsNullOrEmpty(Configuration.GetConnectionString("LocalLegalHomeDB")))
                tenantConnectionString = Configuration.GetConnectionString("LocalLegalHomeDB");

            //TODO Move this to an extension method
            services.AddHttpClient(HttpClientNames.IdPDiscovery, httpClient =>
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1) }));

            services.AddControllers(options =>
                        {
                            options.AllowEmptyInputInBodyModelBinding = true;
                        })
                    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
            services.AddScoped<IDataMappingFactory, DataMappingFactory>();
            services.AddScoped<DataMapping>();
            services.AddScoped<IDataMapping, DataMapping>(s => s.GetService<DataMapping>());
            services.AddScoped<DataMappingOAuth>();
            services.AddScoped<IDataMapping, DataMappingOAuth>(s => s.GetService<DataMappingOAuth>());
            services.AddScoped<DataMappingBasic>();
            services.AddScoped<IDataMapping, DataMappingBasic>(s => s.GetService<DataMappingBasic>());
            services.AddScoped<DataMappingTransfer>();
            services.AddScoped<IDataMapping, DataMappingTransfer>(s => s.GetService<DataMappingTransfer>());
            services.AddScoped<DataMappingOAuthWithTransfer>();
            services.AddScoped<IDataMapping, DataMappingOAuthWithTransfer>(s => s.GetService<DataMappingOAuthWithTransfer>());
            services.AddScoped<IDataMappingService, DataMappingService>();
            services.AddScoped<IConnectorService, ConnectorService>();
            services.AddScoped<IDataMappingRepository, DataMappingRepository>();
            services.AddScoped<IRepository, Repository>();
            services.AddScoped<IRedisRepository, RedisRepository>();
            services.AddScoped<IConnectorDataSourceOperations, IManageConnectorOperations>();
            services.AddScoped<IConnectorDataSourceOperations, IManageOnPremConnectorOperations>();
            services.AddScoped<IConnectorDataSourceOperations, LexisNexisUKConnectorOperation>();
            services.AddScoped<IConnectorDataSourceOperations, MsGraphConnectorOperation>();
            services.AddScoped<IConnectorDataSourceOperations, NetDocsConnectorOperation>();
            services.AddScoped<IConnectorDataSourceOperations, PracticalLawConnectorOperation>();
            services.AddScoped<IConnectorDataSourceOperations, ThoughtTraceOntologiesConnectorOperation>();
            services.AddScoped<IConnectorDataSourceOperations, WestLawIEConnectorOperation>();
            services.AddScoped<IConnectorDataSourceOperations, WestLawUKConnectorOperation>();
            services.AddScoped<IConnectorDataSourceOperationsFactory, ConnectorDataSourceOperationsFactory>();
            services.AddScoped<IHttpBodyFactory, HttpBodyFactory>();
            services.AddScoped<HttpGetBodyService>();
            services.AddScoped<IHttpBodyService, HttpGetBodyService>(s => s.GetService<HttpGetBodyService>());
            services.AddScoped<HttpPostBodyService>();
            services.AddScoped<IHttpBodyService, HttpPostBodyService>(s => s.GetService<HttpPostBodyService>());
            services.AddScoped<IHttpResponseFactory, HttpResponseFactory>();
            services.AddScoped<HttpJsonResponseService>();
            services.AddScoped<IHttpResponseService, HttpJsonResponseService>(s => s.GetService<HttpJsonResponseService>());
            services.AddScoped<HttpXmlResponseService>();
            services.AddScoped<IHttpResponseService, HttpXmlResponseService>(s => s.GetService<HttpXmlResponseService>());
            services.AddScoped<HttpXslResponseService>();
            services.AddScoped<IHttpResponseService, HttpXslResponseService>(s => s.GetService<HttpXslResponseService>());


            services.AddServices();
            services.AddDiagnostics();
            services.AddSwaggerGen(c =>
            {
                if (!c.SwaggerGeneratorOptions.SwaggerDocs.Keys.Contains("v1"))
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Zurich.Connector.Web", Version = "v1" });
                }
                c.ResolveConflictingActions(x => x.First());
            });
            var apiVersionBuilder = services.AddApiVersioning(
               options =>
               {
                   // reporting api versions will return the headers "api-supported-versions" and "api-deprecated-versions"
                   options.ReportApiVersions = true;
                   options.AssumeDefaultVersionWhenUnspecified = true;
               });
            apiVersionBuilder.AddApiExplorer(
                options =>
                {
                    // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
                    // note: the specified format code will format the version as "'v'major[.minor][-status]"
                    options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
                });

            var corsOrigins = Configuration.GetSection("AllowedCORSOrigins")?.Get<string[]>();
            if (corsOrigins != null && corsOrigins.Length > 0)
            {
                services.AddCors(options =>
                {
                    options.AddPolicy(name: CORSPolicies.DefaultPolicy,
                        builder =>
                        {
                            builder
                                .SetIsOriginAllowedToAllowWildcardSubdomains()
                                .WithOrigins(corsOrigins)
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .AllowCredentials();
                        });
                });
            }

            services.AddPartnerAppAuth(tenantConnectionString, _legalPlatformAuthOptions.TokenIssuer, _oAuthOptions, _microServOptions);
            services.AddAutoMapper(typeof(CommonMappingsProfile), typeof(ServiceMappingRegistrar), typeof(MappingRegistrar));
            services.AddConnectorCosmosServices(_connectorCosmosDbOptions, _connectorCosmosClientOptions);
            services.ConfigureExceptonhandler();
            services.AddOAuthHttpClient(Configuration.GetValue<string>(AppSettings.OAuthUrl));
            services.AddAppConfigServices(Configuration.GetValue<string>("splitIOApiKey"));
            services.AddInternalAPIHttpClient(HttpClientNames.IHDocumentStorage, Configuration.GetValue<string>("IHDocumentStorageBaseUrl"));
            services.AddApplicationInsightsTelemetry(Configuration);
            services.AddRedisClient(Configuration);

            var license = Configuration.GetValue<string>("AsposeLicence");
            AsposeLicenceProvider.SetAsposeWordsLicense(license);
            AsposeLicenceProvider.SetAsposePDFLicense(license);

            AddAuthServices(services, _legalPlatformAuthOptions, _ciamAuthOptions);
        }

        public virtual void AddAuthServices(IServiceCollection services, AuthIssuerOptions legalPlatformAuthOptions, CIAMAuthOptions ciamOptions)
        {
            services.AddAuthenticationServices(legalPlatformAuthOptions, ciamOptions);

            // Policy for Service Connector which should have the sufficient claim to make call to the endpoint.
            services.AddAuthorization(x => x.AddPolicy(Policies.ServiceConnectorPolicy, x =>
             {
                 x.AuthenticationSchemes.Add(AuthSchemes.LegalPlatform);
                 x.RequireClaim(JwtClaimTypes.Scope, legalPlatformAuthOptions.AppScopes);
             }));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            RegisterColumnEncryptionProvider(app);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.ConfigureExceptionHandleMiddleware(env);
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Zurich.Connector.Web v1");
            });
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors(CORSPolicies.DefaultPolicy);
            app.UseAuthorization();
            app.UseClaimsTransformation();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapControllers();
            });
        }

        /// <summary>
        /// Initializes the data store for IdentityServer related entities and registers the column encryption key store provider
        /// </summary>
        /// <param name="app">The application request pipeline builder</param>
        protected void RegisterColumnEncryptionProvider(IApplicationBuilder app)
        {
            var clientSecretCredential = new ClientSecretCredential(_azureOptions.ApplicationTenant, _azureOptions.ClientId, _azureOptions.ClientSecret);

            SqlColumnEncryptionAzureKeyVaultProvider azureKeyVaultProvider = new(clientSecretCredential);
            var providers = new Dictionary<string, SqlColumnEncryptionKeyStoreProvider>
            {
                { SqlColumnEncryptionAzureKeyVaultProvider.ProviderName, azureKeyVaultProvider }
            };

            SqlConnection.RegisterColumnEncryptionKeyStoreProviders(providers);
        }

        /// <summary>
        /// Bind configuration options
        /// </summary>
        private void BindConfigurationOptions()
        {
            _azureOptions = new AzureAdOptions();
            Configuration.Bind("AzureAd", _azureOptions);

            _oAuthOptions = new OAuthOptions();
            Configuration.Bind("PartnerOAuthApps", _oAuthOptions);

            _microServOptions = new MicroServiceOptions();
            Configuration.Bind("HighqMS", _microServOptions);

            _connectorCosmosDbOptions = new CosmosDbOptions();
            Configuration.Bind("CosmosDb", _connectorCosmosDbOptions);

            _connectorCosmosClientOptions = new CosmosClientSettings();
            Configuration.Bind("Connector:CosmosClientOptions", _connectorCosmosClientOptions);

            _legalPlatformAuthOptions = new AuthIssuerOptions();
            Configuration.Bind("LegalPlatform", _legalPlatformAuthOptions);

            _ciamAuthOptions = new CIAMAuthOptions();
            Configuration.Bind("CIAM", _ciamAuthOptions);
        }
    }
}
