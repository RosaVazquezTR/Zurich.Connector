﻿using Microsoft.AspNetCore.Authentication;
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
            var claims = new[] {
            new Claim(ClaimTypes.Name, "IntegrationTest User"),
            new Claim(ClaimTypes.NameIdentifier, "IntegrationTest User"),
        };
            var identity = new ClaimsIdentity(claims, "IntegrationTest");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "IntegrationTest");
            var result = AuthenticateResult.Success(ticket);
            return Task.FromResult(result);
        }

    }

   
}
