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
		public bool GuestTenantMember { get; }

		public string ClientId { get; }


		public IntegrationTestSessionAccessor()
		{
			SessionId = "testSession";
			UserId = new Guid("f68281a6-f474-4069-a61f-1ff6e92e1655");
			Email = "integrationTestUser@mailinator.com";
			UserName = "integrationTestUser";
			TenantId = new Guid("b891c0c2-cdef-453f-af8f-08d919703e5c");
			OrgType = "Connector Integration Test";
			GuestTenantMember = false;
			ClientId = "";

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
