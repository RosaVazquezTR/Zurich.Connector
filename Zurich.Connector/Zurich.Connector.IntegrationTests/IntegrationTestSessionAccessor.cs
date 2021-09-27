using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Zurich.Common.Services;
using Zurich.TenantData;

namespace Zurich.Connector.IntegrationTests
{
    public class IntegrationTestSessionAccessor : ISessionAccessor
	{
		public string SessionId { get; internal set; }

		public Guid UserId { get; set; }

		public string Email { get; internal set; }

		public string UserName { get; internal set; }

		public Guid TenantId { get; set; }

		public string OrgType { get; set; }



		public IntegrationTestSessionAccessor()
		{
			SessionId = "testSession";
			UserId = new Guid("b96df8d1-2277-48fd-8311-074c92689a20");
			Email = "hqhomeuser21@mailinator.com";
			UserName = "hqhomeuser21";
			TenantId = new Guid("e08ea8e1-35b8-42a7-ca6e-08d83973458f");
			OrgType = "Connector Integration Test";
		}

		/// <summary>
		/// Will populate information around Email, userName, and OrgType.  
		/// Makes a call to get userInfo from IS
		/// </summary>
		/// <returns>Empty Task (Values are updated on the object)</returns>
		public async Task PopulateUserInfo()
		{

		}

		public Task<ClaimsPrincipal> Authenticate(HttpRequest request)
		{
			throw new NotImplementedException();
		}

	}
}
