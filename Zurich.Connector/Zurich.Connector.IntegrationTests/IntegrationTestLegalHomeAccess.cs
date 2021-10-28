using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.Data;

namespace Zurich.Connector.IntegrationTests
{
    public class IntegrationTestLegalHomeAccess : ILegalHomeAccessCheck
    {
        public bool isLegalHomeUser()
        {
            return true;
        }
    }
}
