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
			UserId = new Guid("07fdf923-8dba-4491-854c-b9b4511a0d6c");
			Email = "integrationTestUser@mailinator.com";
			UserName = "integrationTestUser";
			TenantId = new Guid("22657dbe-f5f4-4637-af94-08d919703e5c");
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
