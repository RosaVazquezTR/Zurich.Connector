using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Zurich.Connector.Data
{
    public interface IntegrationHubAccessCheck
    {
        public bool isIntegrationHubUser();
    }

    public class IntegrationHubAccess : IntegrationHubAccessCheck
    {
        protected IHttpContextAccessor _contextAccessor;

        public IntegrationHubAccess(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        public bool isIntegrationHubUser()
        {
            var principal = _contextAccessor?.HttpContext?.User;

            if (principal != null)
            {
                bool hasScope = principal.Claims.Any();
                if (hasScope)
                {
                    bool containsScope = true; 
                    var scopes = principal.FindAll("scope").Select(scope => scope.Value);
                    foreach (var IHscope in DataConstants.IntegrationHubScopes) 
                    {
                        containsScope = containsScope && scopes.Contains(IHscope);
                    }
                    return containsScope;

                }
            }
            return false;
        }
    }

}
