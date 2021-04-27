using System;
using System.IdentityModel.Tokens.Jwt;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Polly;
using Zurich.Common;
using Zurich.Common.Models.HighQ;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Services.Security;
using Zurich.Connector.Web.Configuration;

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
		public static void AddPartnerAppAuth(this IServiceCollection services, string tenantConnectionString, string productsConnectionString, OAuthOptions oAuthOptions, MicroServiceOptions microServiceOptions)
		{
			services.AddSingleton(oAuthOptions);
			services.AddSingleton(microServiceOptions);
			services.AddHttpClient(HttpClientNames.OAuth, httpClient =>
			{
				httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
			})
			.AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(10) }));

            services.AddHttpClient(HttpClientNames.HighQ, httpClient =>
            {
                httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(new[] { TimeSpan.FromSeconds(1) }));

            services.AddScoped<IEncryptionService, EncryptionService>();
            services.AddScoped<IOAuthService, OAuthService>();
            services.AddScoped<IOAuthStore, OAuthTenantStore>();

            services.AddHttpContextAccessor();
            services.AddCommonTenantServices(tenantConnectionString, oAuthOptions);
            services.AddDefaultProductsDbConnection(productsConnectionString);
            services.AddProductServices();

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
	}
}