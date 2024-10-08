﻿using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Repositories;
using Zurich.Connector.Data;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.TenantData;
using static Zurich.Connector.Data.Constants;
using AppModel = Zurich.Connector.App.Model;
using CommonModel = Zurich.Common.Models.Connectors;
using CommonServices = Zurich.Common.Services;

namespace Zurich.Connector.App.Services
{
    public interface IRegistrationService
    {
        /// <summary>
        /// Registers a datasource to a given user
        /// </summary>
        /// <param name="connectorId">The connector id</param>
        /// <param name="domain">The connector domain</param>
        /// <returns>DataSourceRegistration</returns>
        Task<DataSourceRegistration> RegisterConnector(string connectorId, string domain);

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
        private readonly ISessionAccessor _sessionAccessor;
        private readonly IOAuthServices _OAuthService;
        private readonly IOAuthApiRepository _oAuthApiRepository;
        private readonly IConfiguration _configuration;
        private readonly CommonServices.ITenantService _tenantService;
        private readonly ILegalHomeAccessCheck _legalHomeAccess;
        private readonly OAuthOptions _oAuthOptions;
        private readonly TelemetryClient _telemetry;

        public RegistrationService(ICosmosService cosmosService, ISessionAccessor sessionAccessor, IOAuthServices OAuthService, IOAuthApiRepository OAuthApiRepository, IConfiguration configuration, CommonServices.ITenantService tenantService, ILegalHomeAccessCheck legalHomeAccess, OAuthOptions oauthOptions, TelemetryClient telemetryClient)
        {
            _cosmosService = cosmosService;
            _sessionAccessor = sessionAccessor;
            _OAuthService = OAuthService;
            _oAuthApiRepository = OAuthApiRepository;
            _configuration = configuration;
            _tenantService = tenantService;
            _legalHomeAccess = legalHomeAccess;
            _oAuthOptions = oauthOptions;
            _telemetry = telemetryClient; 
        }

        public async Task<DataSourceRegistration> RegisterConnector(string connectorId, string domain = null)
        {
            if (string.IsNullOrEmpty(connectorId))
            {
                return null;
            }
         
            var connector = await _cosmosService.GetConnectorUsingPreRelease(connectorId, true);

            if (string.IsNullOrEmpty(domain))
            {
                domain = connector.DataSource.Domain;
            }

            var dataSourceRegistration = await RegisterDataSource(connector.DataSource.AppCode, domain, connector.DataSource);

            return dataSourceRegistration;
        }

        public async Task<DataSourceRegistration> RegisterDataSource(string applicationCode, string domain, AppModel.DataSourceModel dataSource = null)
        {
            DataSourceRegistration response = new DataSourceRegistration();

            // if a user already registered just return true
            IEnumerable<DataSourceInformation> currentUserRegistrations = await _OAuthService.GetUserRegistrations();

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

            // For the specific case of the iManage, if a private domain is not provided, populate it with the public stored in config
            // As iManage is the only application that actually handle a different domain than the stored one, we explicitly validate it
            if (applicationCode.Equals("iManage") && (dataSource.RegistrationInfo.DomainRequired && string.IsNullOrWhiteSpace(domain)))
            {
                var appinfoDetails = _oAuthOptions.Connections.SingleOrDefault(x => x.Key.Equals(applicationCode, StringComparison.OrdinalIgnoreCase));
                domain = appinfoDetails.Value.BaseUrl;
            }

            if (string.Equals(applicationCode, AppCodes.TTTenantApp))
            {
                var appTenantDetails = await _OAuthService.GetDatasourceTenantInfo(applicationCode);
                if (String.IsNullOrWhiteSpace(appTenantDetails.AppTenantId))
                {
                    _telemetry.TrackTrace($"Unable to register connector. DI Tenat not registered for LPAS TenantId {appTenantDetails.TenantId}", SeverityLevel.Warning);
                    throw new InvalidOperationException($"Unable to register connector. DI Tenat not registered for user TenantId");
                }
            }

            // if datasource not exists or domain is required and not provided 
            if (dataSource == null || (dataSource.RegistrationInfo.DomainRequired && string.IsNullOrEmpty(domain)))
            {
                return response;
            }

            // Check to see if we can register this value
            List<DataSourceInformation> availableRegistrations = await _OAuthService.GetAvailableRegistrations();
            //As iManage can actually handle a different domain than the stored one, we exclude it from following validation
            if (!availableRegistrations.Any(x => x.AppCode.Equals(applicationCode, StringComparison.OrdinalIgnoreCase)
                && (!dataSource.RegistrationInfo.DomainRequired || domain.Equals(x.Domain, StringComparison.OrdinalIgnoreCase)|| applicationCode.Equals("iManage"))))
            {
                    return response;
            }

            bool isAutomaticRegistration = false;
            switch (dataSource.RegistrationInfo.RegistrationMode)
            {
                case CommonModel.RegistrationEntityMode.Automatic:
                    isAutomaticRegistration = true;
                    break;
                case CommonModel.RegistrationEntityMode.TenantWide:
                    OAuthAPITokenResponse token;
                    if (_sessionAccessor.UserId == Guid.Empty)
                        token = await _oAuthApiRepository.GetTokenWithTenantId(applicationCode, _sessionAccessor.TenantId.ToString(), null);
                    else
                        token = await _oAuthApiRepository.GetToken(applicationCode);

                    isAutomaticRegistration = token != null;
                    break;
            }

            if (isAutomaticRegistration)
            {
                response.Registered = await _OAuthService.AutomaticRegistration(applicationCode);
                return response;
            }
            else
            {
                var oAuthDomain = _configuration.GetValue<string>(AppSettings.OAuthUrl);
                response.Registered = false;
                response.AuthorizeUrl = $"{oAuthDomain}api/v1/{applicationCode}/authorizeURL?domain={domain}";
                return response;
            }
        }

        public async Task<bool> RemoveUserConnector(string connectorId)
        {
            var userId = _sessionAccessor.UserId.ToString();

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
                return await _OAuthService.GetUserRegistrations();
            }
        }
    }
}
