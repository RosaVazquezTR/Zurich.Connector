using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CommonModel = Zurich.Common.Models.Connectors;
using CommonServices = Zurich.Common.Services;
using Zurich.Connector.Data;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using DataServices = Zurich.Connector.Data.Services;
using AppModel = Zurich.Connector.App.Model;
using Zurich.TenantData;

namespace Zurich.Connector.App.Services
{
    public interface IRegistrationService
    {
        /// <summary>
        /// Registers a datasource to a given user
        /// </summary>
        /// <param name="connectorId">The connector id</param>
        /// <returns>DataSourceRegistration</returns>
        Task<DataSourceRegistration> RegisterConnector(string connectorId);

        /// <summary>
        /// Registers data source
        /// </summary>
        /// <param name="applicationCode">The application code</param>
        /// <param name="domain">The Domain</param>
        /// <param name="dataSourceModel">The DataSource of the connector</param>
        /// <returns></returns>
        Task<DataSourceRegistration> RegisterDataSource(string applicationCode, string domain, AppModel.DataSourceModel dataSourceModel);

        /// <summary>
        /// Remove user from cosmosdb
        /// </summary>
        /// <param name="connectorId">The connector id</param>
        /// <returns>Boolean indicating success</returns>
        Task<bool> RemoveUserConnector(string connectorId);

        /// <summary>
        /// Gets list of user's registered connector ids
        /// </summary>
        /// <param name="registrationModes">List of registration modes to provide additional filtering</param>
        /// <returns>List of connector ids</returns>
        Task<IEnumerable<DataSourceInformation>> GetUserDataSources();
    }

    public class RegistrationService : IRegistrationService
    {
        private readonly ICosmosService _cosmosService;
        private readonly ISessionAccessor _sessionAccesor;
        private readonly IOAuthServices _OAuthService;
        private readonly IConfiguration _configuration;
        private readonly CommonServices.ITenantService _tenantService;
        private readonly ILegalHomeAccessCheck _legalHomeAccess;

        public RegistrationService(ICosmosService cosmosService, ISessionAccessor sessionAccesor, IOAuthServices OAuthService, IConfiguration configuration, CommonServices.ITenantService tenantService, ILegalHomeAccessCheck legalHomeAccess)
        {
            _cosmosService = cosmosService;
            _sessionAccesor = sessionAccesor;
            _OAuthService = OAuthService;
            _configuration = configuration;
            _tenantService = tenantService;
            _legalHomeAccess = legalHomeAccess;
        }

        public async Task<DataSourceRegistration> RegisterConnector(string connectorId)
        {
            if (string.IsNullOrEmpty(connectorId))
            {
                return null;
            }

            var connector = await _cosmosService.GetConnectorUsingPreRelease(connectorId, true);

            var dataSourceRegistration = await RegisterDataSource(connector.DataSource.AppCode, connector.DataSource.Domain, connector.DataSource);

            return dataSourceRegistration;
        }

        public async Task<DataSourceRegistration> RegisterDataSource(string applicationCode, string domain, AppModel.DataSourceModel dataSource = null)
        {
            DataSourceRegistration response = new DataSourceRegistration();

            // if a user already registered just return true
            List<DataSourceInformation> currentUserRegistrations = await _OAuthService.GetUserRegistrations();
            if (currentUserRegistrations != null && currentUserRegistrations.Any(x => x.AppCode.Equals(applicationCode, StringComparison.OrdinalIgnoreCase)))
            {
                response.Registered = true;
                return response;
            }

            // Get the datasource from Cosmos
            if(dataSource == null)
            {
                Expression<Func<DataSourceDocument, bool>> dsCondition = dataSource => dataSource.appCode.Equals(applicationCode, StringComparison.OrdinalIgnoreCase);
                dataSource = (await _cosmosService.GetDataSources(dsCondition)).SingleOrDefault();
            }
           
            if (dataSource == null || (dataSource.RegistrationInfo.DomainRequired && string.IsNullOrEmpty(domain)))
            {
                return response;
            }

            // Check to see if we can register this value
            List<DataSourceInformation> availableRegistrations = await _OAuthService.GetAvailableRegistrations();
            if (!availableRegistrations.Any(x => x.AppCode.Equals(applicationCode, StringComparison.OrdinalIgnoreCase)
                && (!dataSource.RegistrationInfo.DomainRequired || domain.Equals(x.Domain, StringComparison.OrdinalIgnoreCase))))
            {
                return response;
            }

            if (dataSource.RegistrationInfo.RegistrationMode == CommonModel.RegistrationEntityMode.Automatic)
            {
                response.Registered = await _OAuthService.AutomaticRegistration(applicationCode);
                return response;
            }
            //Manual registration that is not registered returned auth url
            //AuthorizeUrlResponse authUrlResponse = await _OAuthService.GetAuthorizeUrl(dataSource.AppCode);

            var oAuthDomain = _configuration.GetValue<string>(AppSettings.OAuthUrl);
            response.Registered = false;
            response.AuthorizeUrl = $"{oAuthDomain}api/v1/{applicationCode}/authorizeURL";
            return response;
        }

        public async Task<bool> RemoveUserConnector(string connectorId)
        {
            var userId = _sessionAccesor.UserId.ToString();

            Expression<Func<ConnectorRegistrationDocument, bool>> condition = registration => registration.ConnectorId == connectorId;
            var registrations = _cosmosService.GetConnectorRegistrations(userId, condition);
            if (registrations.Count() < 1)
            {
                return false;
            }

            foreach (var registration in registrations)
            {
                await _cosmosService.RemoveConnectorRegistration(registration.Id, userId);
            }
            return true;
        }

        public async Task<IEnumerable<DataSourceInformation>> GetUserDataSources()
        {
            if (_legalHomeAccess.isLegalHomeUser())
            {
                var tenantApps = await _tenantService.GetTenantMemberApps();
                var dataSourceAppCodes = tenantApps.Where(x=>x.CurrentToken_Id != null).Select(tenantApp => new DataSourceInformation() { AppCode = tenantApp.ApplicationCode }).ToList();

                var dataSources = await _cosmosService.GetDataSources();
                // TODO figure out why condition does not work enums
                dataSources = dataSources.Where(x => x.RegistrationInfo?.RegistrationMode == CommonModel.RegistrationEntityMode.Automatic);
                // NOTE returning RequiresNewToken as false for Legal Home Users until Legal Home incorporates new OAuth flow updates.
                // Applying Distinct couldn't drop duplicate datasource entries, Changed to GroupBy and selected the first element.  
                dataSourceAppCodes.AddRange(dataSources.Where(x => !string.IsNullOrEmpty(x.AppCode)).Select(x => new DataSourceInformation() { AppCode = x.AppCode, RequiresNewToken = false }).GroupBy(x => x.AppCode).Select(x => x.First()));

                return dataSourceAppCodes;
            }
            else
            {
                List<DataSourceInformation> currentUserRegistrations = await _OAuthService.GetUserRegistrations();
                return currentUserRegistrations;
            }
        }
    }
}
