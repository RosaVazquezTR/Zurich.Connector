using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using AutoMapper;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Zurich.Common;
using Zurich.Common.Exceptions;
using Zurich.Common.Middleware;
using Zurich.Common.Models.Cosmos;
using Zurich.Common.Models.HighQ;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Repositories.Cosmos;
using Zurich.Common.Services;
using Zurich.Common.Services.Security;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Web.Configuration;
using Zurich.TenantData;
namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Represents an extension class used for bootstrapping the required services
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds partner app authentication related services
		/// </summary>
		/// <param name="tenantConnectionString">The Legal Home database connection string</param>
		/// <param name="productsConnectionString">The products database connection string</param>
		/// <param name="services">The app service collection</param>
		/// <param name="oAuthOptions">The OAuth partner app connections details</param>
		public static void AddPartnerAppAuth(this IServiceCollection services, string tenantConnectionString, string authority, OAuthOptions oAuthOptions, MicroServiceOptions microServiceOptions)
		{
			services.AddSingleton(microServiceOptions);

			services.AddHttpClient(HttpClientNames.HighQ, httpClient =>
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1) }));

            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddHttpContextAccessor();
			services.AddCommonTenantServicesWithWebApiAuth(tenantConnectionString, authority, oAuthOptions);

        }

		/// <summary>
		/// Adds services to dependency injection
		/// </summary>
		/// <param name="services">The app service collection</param>
		public static void AddServices(this IServiceCollection services)
		{
			services.AddScoped<IRegistrationService, RegistrationService>();
		}

		/// <summary>
		/// Adds diagnostics related services
		/// </summary>
		/// <param name="services">The app service collection</param>
		public static void AddDiagnostics(this IServiceCollection services)
        {
			services.AddApplicationInsightsTelemetry();
			services.AddHealthChecks();
        }

		/// <summary>
		/// Adds authentication related services
		/// </summary>
		/// <param name="services">The app services</param>
		/// <param name="audience">The authorization token audience</param>
		/// <param name="authority">The authorization token issuer</param>
		public static void AddAuthenticationServices(this IServiceCollection services, string audience, string authority)
		{
			JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
			{
				options.Authority = authority;
				options.Audience = audience;
				options.TokenValidationParameters.ValidTypes = new[] { SupportedTokenTypes.AccessTokenJwt };
			});

			services.AddAuthorization(options =>
			{
				// TODO: Add Connector specific scopes to Identity Server
				options.FallbackPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser().RequireClaim(JwtClaimTypes.Scope, SupportedScopes.Full).Build();
			});
		}

		/// <summary>
		/// Configure dependency injection services for cosmos module.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="dbOptions"> cosmos DB options</param>
		/// <param name="clientOptions"> cosmos client options</param>
		public static void AddConnectorCosmosServices(this IServiceCollection services, CosmosDbOptions dbOptions, CosmosClientSettings clientOptions)
        {
			List<CosmosClient> clients = new List<CosmosClient>();
			var clientSettings = new CosmosClientOptions()
			{
				AllowBulkExecution = clientOptions.AllowBulkExecution,
				ConnectionMode = ConnectionMode.Gateway,
				GatewayModeMaxConnectionLimit = clientOptions.GatewayModeMaxConnectionLimit == 0 ? 10 : clientOptions.GatewayModeMaxConnectionLimit,
				MaxRetryAttemptsOnRateLimitedRequests = clientOptions.MaxRetryAttemptsOnRateLimitedRequests == 0 ? 9 : clientOptions.MaxRetryAttemptsOnRateLimitedRequests,
				MaxRetryWaitTimeOnRateLimitedRequests = clientOptions.MaxRetryWaitTimeOnRateLimitedRequests == 0 ? new TimeSpan(0, 0, 30) : new TimeSpan(0, 0, clientOptions.MaxRetryWaitTimeOnRateLimitedRequests),
                SerializerOptions = new CosmosSerializationOptions() { PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase }
            };

            clients.Add(new CosmosClient(dbOptions.Endpoint, dbOptions.PrimaryKey, clientSettings));
            services.AddSingleton<IEnumerable<CosmosClient>>(clients);
            services.AddSingleton<ICosmosClientFactory, CosmosClientFactory>();
            services.AddScoped(sp => new ConnectorCosmosContext(sp.GetRequiredService<ICosmosClientFactory>(), dbOptions));
			services.AddTransient<ICosmosService, CosmosService>();
        }

		/// <summary>
		/// Configuring the ExceptionHandling Middleware.
		/// </summary>
		public static void ConfigureExceptionHandleMiddleware(this IApplicationBuilder App, IHostEnvironment env)
        {
			App.UseMiddleware<ExceptionHandlingMiddleware>();
        }

		/// <summary>
		/// Adding Exception handler dependency class from Common package.
		/// </summary>
		public static void ConfigureExceptonhandler(this IServiceCollection services)
        {
			services.AddSingleton<IExceptionHandler, ExceptionHandler>();
		}
		}

}