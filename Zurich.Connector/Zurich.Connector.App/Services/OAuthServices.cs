using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;

namespace Zurich.Connector.App.Services
{

    public interface IOAuthServices
    {
        /// <summary>
        /// Get the Automatic registration status from OAuth Service
        /// </summary>
        /// <param name="appCode">using appCode parameter passing proper application code.</param>
        /// <returns>Return the Status of autoregistration</returns>
        Task<bool> AutomaticRegistration(string appCode);

        /// <summary>
        /// Lists out the registrations for a user that can be registered
        /// </summary>
        /// <returns>returns the information around the domain, appcode and base url</returns>
        Task<List<DataSourceInformation>> GetAvailableRegistrations();

        /// <summary>
        /// Gets a users current dataSource registrations
        /// </summary>
        /// <returns>returns the information around the domain, appcode and base url for user registrations</returns>
        Task<IEnumerable<DataSourceInformation>> GetUserRegistrations();

        /// <summary>
        /// Using an appCode will return the corresponding authorize endpoint so the user can sign in
        /// </summary>
        /// <param name="applicationCode">The application to get the authorize url for</param>
        /// <returns>Authorize url</returns>
        Task<AuthorizeUrlResponse> GetAuthorizeUrl(string applicationCode);

        /// <summary>
        /// Revoke Tenanat Application for the appropriate connector 
        /// </summary>
        /// <param name="applicationCode">The application code of the connector</param>
        /// <returns>Revoking of Tenanat application success or failure status</returns>
        Task<bool> RevokeTenantApplication(string applicationCode);
    }
    public class OAuthServices : IOAuthServices
    {
        private readonly IOAuthRepository _OAuthRespository;

        public OAuthServices(IOAuthRepository OAuthRepository)
        {
            _OAuthRespository = OAuthRepository;
        }
        public async Task<bool> AutomaticRegistration(string appCode)
        {
            return await _OAuthRespository.AutomaticRegistration(appCode);
        }
        public async Task<List<DataSourceInformation>> GetAvailableRegistrations()
        {
            return await _OAuthRespository.GetAvailableRegistrations();
        }
        public async Task<IEnumerable<DataSourceInformation>> GetUserRegistrations()
        {
            return await _OAuthRespository.GetUserRegistrations();
        }
        public async Task<AuthorizeUrlResponse> GetAuthorizeUrl(string applicationCode)
        {
            return await _OAuthRespository.GetAuthorizeUrl(applicationCode);
        }

        public async Task<bool> RevokeTenantApplication(string applicationCode)
        {
            return await _OAuthRespository.RevokeTenantApplication(applicationCode);
        }
    }
}
