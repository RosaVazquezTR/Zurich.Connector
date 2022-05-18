using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Zurich.Common;
using Zurich.Common.Testing;

namespace Zurich.Connector.IntegrationTests
{
    public class Helper: IClassFixture<CustomWebApplicationFactory>
    {
        protected IConfiguration _configuration;
        protected IConfigurationBuilder _configBuilder;
        protected readonly ILogger _logger;
        protected readonly CustomWebApplicationFactory _factory;
        protected readonly HttpClient _client;

        public Helper()
        {
            string json = "integrationsettings.json";

            IConfigurationBuilder builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile(json, optional: true);
            _configuration = builder.Build();

            string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? _configuration["ASPNETCORE_ENVIRONMENT"];

            string envJson = $"integrationsettings.{env}.json";

            // TODO: arguably we should be able to use the same builder as above but the tests seem to break when running on the build machine
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(json, optional: true)
                .AddJsonFile(envJson, optional: true)
                .AddUserSecrets(Assembly.GetExecutingAssembly())
                .Build();

            builder.AddAzureKeyVault(
                _configuration["KeyVault:Endpoint"],
                _configuration["AzureAd:ClientId"],
                _configuration["AzureAd:ClientSecret"]);

            builder.AddAzureKeyVault(
                _configuration["KeyVault:SearchEndpoint"],
                _configuration["AzureAd:ClientId"],
                _configuration["AzureAd:ClientSecret"]);

            _configuration = builder.Build();
            _logger = LoggerFactory.Create(b => b.AddDebug()
                                                 .AddConsole()).CreateLogger<TokenHelper>();
        }

        public TokenRequest GetTokenRequestDetails(string user)
        {
            TokenRequest tokenRequest = null;
            try
            {
                if(_configuration !=null)
                {
                    tokenRequest = new TokenRequest();
                    tokenRequest.IdentityServerEndpoint = _configuration.GetValue<string>("IdentityServer:IdentityServerEndpoint");
                    tokenRequest.IdentityServerclientSecret = _configuration.GetValue<string>("AzureAd:ClientSecret");
                    tokenRequest.Username = _configuration.GetValue<string>("onePassUsername:" + user);
                    tokenRequest.Password = _configuration.GetValue<string>("OnePassPassword:" + user);
                    tokenRequest.IdentityServerclientId = _configuration.GetValue<string>("IdentityServer:ClientId");
                    tokenRequest.IdentityServerclientSecret = _configuration.GetValue<string>("IdentityServer:ClientSecret");
                    tokenRequest.GrantType = _configuration.GetValue<string>("IdentityServer:GrantType");
                    tokenRequest.Scope = _configuration.GetValue<string>("IdentityServer:Scope");
                }
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
            return tokenRequest;
        }


        public HttpRequestMessage TokenRequest(string request)
        {
            var host = _configuration.GetValue<string>("IntegrationTestHosts:Host").ToString();
            request = $"{host}{request}";

            var token = new TokenHelper().GetAuthToken(new Helper().GetTokenRequestDetails("RecentOptedInUser"));

            var getRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(request),
            };
            getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.access_token);
            return getRequest;
        }

    }
}
