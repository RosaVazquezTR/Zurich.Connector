using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Zurich.Connector.Data
{
    public interface ILegalHomeAccessCheck
    {
        public bool isLegalHomeUser();
    }

    public class LegalHomeAccess : ILegalHomeAccessCheck
    {
        protected IHttpContextAccessor _contextAccessor;

        public LegalHomeAccess(IHttpContextAccessor contextAccessor)
        {
            _contextAccessor = contextAccessor;
        }
        public bool isLegalHomeUser()
        {
            var principal = _contextAccessor?.HttpContext?.User;

            if (principal != null)
            {
                bool hasScope = principal.Claims.Any();
                if (hasScope)
                {
                    var scopes = principal.FindAll("scope").Select(scope => scope.Value);
                    return scopes.Contains(DataConstants.LegalHomeScope);
                }
            }
            return false;
        }
    }

}
