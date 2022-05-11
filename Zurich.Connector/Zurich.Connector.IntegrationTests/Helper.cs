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

namespace Zurich.Connector.IntegrationTests
{
    public class Helper : IClassFixture<CustomWebApplicationFactory>
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
                                                 .AddConsole()).CreateLogger<Helper>();
        }

        public ISConnectToken GetAuthToken(string user)
        {
            int httpStatusCode = -1;
            ISConnectToken returnResponse = null;
            string errorMessage;
            try
            {
                string IdentityServerUrl = _configuration.GetValue<string>("TokenIssuer");
                string IdentityServerEndpointUrl = _configuration.GetValue<string>("IdentityServer:IdentityServerEndpoint");
                _logger.LogDebug("Sending POST request: " + IdentityServerEndpointUrl);

                var responseContent = string.Empty;

                var keyVault = _configuration.GetValue<string>("KeyVault:Endpoint");
                var clientId = _configuration.GetValue<string>("AzureAd:ClientId");
                var clientSecret = _configuration.GetValue<string>("AzureAd:ClientSecret");
                var onePassUsername = _configuration.GetValue<string>("onePassUsername:" + user);
                var onePassPassword = _configuration.GetValue<string>("OnePassPassword:" + user);

                var identityServerclientId = _configuration.GetValue<string>("IdentityServer:ClientId");
                var identityServerclientSecret = _configuration.GetValue<string>("IdentityServer:ClientSecret");



                string clientIdSecretPair = String.Format("{0}:{1}", identityServerclientId, identityServerclientSecret);
                var byteArray = Encoding.ASCII.GetBytes(clientIdSecretPair);

                using (var client = new HttpClient())
                using (var request = new HttpRequestMessage(HttpMethod.Post, IdentityServerEndpointUrl))
                {
                    request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    IDictionary<string, string> parameters = new Dictionary<string, string>();
                    parameters.Add("username", onePassUsername.ToString());
                    parameters.Add("password", onePassPassword.ToString());
                    parameters.Add("grant_type", _configuration.GetValue<string>("IdentityServer:GrantType"));
                    parameters.Add("scope", _configuration.GetValue<string>("IdentityServer:Scope"));
                    var encodedContent = new FormUrlEncodedContent(parameters);
                    // Set the content of the request message object to your paramaters
                    request.Content = encodedContent;
                    using var response = client.SendAsync(request).Result;
                    response.EnsureSuccessStatusCode();
                    responseContent = response.Content.ReadAsStringAsync().Result;
                    httpStatusCode = (int)response.StatusCode;
                    returnResponse = JsonConvert.DeserializeObject<ISConnectToken>(responseContent);
                }
            }
            catch (HttpRequestException ex)
            {
                errorMessage = "Failed to access token from Identity Server using Identity Server Resource Owner Endpoint.";
                _logger.LogError(errorMessage);
                throw new Exception(errorMessage);
            }
            return returnResponse;
        }

    }
}
