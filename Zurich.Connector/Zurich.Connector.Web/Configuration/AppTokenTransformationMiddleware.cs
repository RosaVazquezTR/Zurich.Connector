using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zurich.Common;

namespace Zurich.Connector.Web.Configuration
{
    /// <summary>
    /// The Request matches with specific URL i.e. api/v{versionNumber}/service/connectors/{id}/data/{tenantId}
    /// and appends the TenantId to the HttpContext. 
    /// This is required in the Client Credential Flow. 
    /// </summary>
    public class AppTokenTransformationMiddleware
    {
        public RequestDelegate _next;

        public AppTokenTransformationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            if (httpContext.User != null)
            {
                if (httpContext.User.Identity.IsAuthenticated)
                {
                    Guid parsed;
                    var url = httpContext.Request.Path.Value;
                    var expectedPattern = @"\/api\/v[0-9]+\/service\/connectors\/(?<id>.+)\/data\/(?<tenant>.+)";
                    Regex rg = new Regex(expectedPattern, RegexOptions.IgnoreCase);
                    var validUrl = rg.Match(url);
                    var tenantId = validUrl.Groups["tenant"].ToString();
                    var validGuid = Guid.TryParse(tenantId, out parsed);
                    if (validUrl.Success && validGuid)
                    {
                        httpContext.User.Identities.FirstOrDefault().AddClaim(new Claim(GeneralClaimTypes.Tenant, tenantId));
                    }
                }

                await _next(httpContext);
            }
        }
    }
}
