using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Xunit;

namespace Zurich.Connector.IntegrationTests
{
    public class IntegrationTestAuthenticationHandler :
         AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public IntegrationTestAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
       ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
       : base(options, logger, encoder, clock)
        {
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var userId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            var sessionId = "f9b5b2db-7d68-4798-acb5-f437fd9445f4";
            var email = "Jhon.Tester@tr.com";
            var name = "Jhon Tester";
            var orgType = "Auto";

            var claims = new Claim[]
            {
                new Claim("sub", userId.ToString()),
                new Claim("tenant", tenantId.ToString()),
                new Claim("session_id", sessionId)
            };
            var identity = new ClaimsIdentity(claims, "IntegrationTest");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "IntegrationTest");
            var result = AuthenticateResult.Success(ticket);
            return Task.FromResult(result);
        }

    }


}
