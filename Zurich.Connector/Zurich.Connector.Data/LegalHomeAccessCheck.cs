using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data
{
    public interface LegalHomeAccessCheck
    {
        public bool isLegalHomeUser();
    }

    public class LegalHomeAccess : LegalHomeAccessCheck
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
                    string scopes = principal.FindFirst("scope").Value;
                    return scopes.Contains(DataConstants.legalHomeScope);
                }
            }
            return false;
        }
    }

}
