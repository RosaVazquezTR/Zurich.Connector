using System;
using AutoMapper;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using Polly;
using Zurich.Common;
using Zurich.Common.Models.Cosmos;
using Zurich.Common.Models.HighQ;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Repositories.Cosmos;
using Zurich.Common.Services.Security;
using Zurich.Connector.App.Services;
using Zurich.Connector.Durable.Service;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// Represents an extension class used for bootstrapping the required services
	/// </summary>
	public static class DurableServiceCollectionExtensions
	{
		/// <summary>
		/// Adds partner app authentication related services
		/// </summary>
		/// <param name="services">The app service collection</param>
		/// <param name="oAuthOptions">The OAuth partner app connections details</param>
		public static void AddPartnerAppAuth(this IServiceCollection services, OAuthOptions oAuthOptions, MicroServiceOptions microServiceOptions)
		{
			services.AddSingleton(oAuthOptions);
			services.AddSingleton(microServiceOptions);
			services.AddHttpClient(HttpClientNames.OAuth, httpClient =>
			{
				httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
			})
			.AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) }));

			services.AddScoped<IEncryptionService, EncryptionService>();
			services.AddScoped<IOAuthService, OAuthService>();
			services.AddScoped<IOAuthStore, OAuthTenantStore>();

		}

		/// <summary>
		/// Configure dependency injection services for cosmos module.
		/// </summary>
		/// <param name="services"></param>
		/// <param name="dbOptions"> cosmos DB options</param>
		/// <param name="clientOptions"> cosmos client options</param>
		public static void AddConnectorCosmosServices(this IServiceCollection services, CosmosDbOptions dbOptions, CosmosClientSettings clientOptions)
		{
			var clientSettings = new CosmosClientOptions()
			{
				AllowBulkExecution = clientOptions.AllowBulkExecution,
				ConnectionMode = ConnectionMode.Gateway,
				GatewayModeMaxConnectionLimit = clientOptions.GatewayModeMaxConnectionLimit == 0 ? 10 : clientOptions.GatewayModeMaxConnectionLimit,
				MaxRetryAttemptsOnRateLimitedRequests = clientOptions.MaxRetryAttemptsOnRateLimitedRequests == 0 ? 9 : clientOptions.MaxRetryAttemptsOnRateLimitedRequests,
				MaxRetryWaitTimeOnRateLimitedRequests = clientOptions.MaxRetryWaitTimeOnRateLimitedRequests == 0 ? new TimeSpan(0, 0, 30) : new TimeSpan(0, 0, clientOptions.MaxRetryWaitTimeOnRateLimitedRequests)
			};

			services.AddCosmosClientStore(dbOptions, clientSettings);
		}
	}
}
