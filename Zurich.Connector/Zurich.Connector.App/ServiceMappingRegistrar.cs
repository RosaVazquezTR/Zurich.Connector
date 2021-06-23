using AutoMapper;
using System;
using Zurich.Connector.App.Model;
using CosmosDocuments = Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.App
{
    /// <summary>
    /// Auto mapper for the service
    /// </summary>
    public class ServiceMappingRegistrar : Profile
    {
        public ServiceMappingRegistrar()
        {
            CreateMap<CosmosDocuments.ConnectorDocument, ConnectorModel>();

            CreateMap<CosmosDocuments.CDMMapping, CdmMapping>();
            CreateMap<CosmosDocuments.CdmElement, CdmElement>();

            CreateMap<CosmosDocuments.ConnectorInfo, ConnectorInfoModel>();

            CreateMap<CosmosDocuments.ConnectorRequest, ConnectorRequestModel>();
            CreateMap<CosmosDocuments.ConnectorRequestParameter, ConnectorRequestParameterModel>();

            CreateMap<CosmosDocuments.DataSourceDocument, DataSourceModel>();

            CreateMap<CosmosDocuments.SecurityDefinition, SecurityDefinitionModel>();
            CreateMap<CosmosDocuments.SecurityDefinitionDetails, SecurityDefinitionDetailsModel>();

            /************************************************************************************************************/
            CreateMap<ConnectorModel, CosmosDocuments.ConnectorDocument>();

            CreateMap<CdmMapping, CosmosDocuments.CDMMapping>();

            CreateMap<CdmElement, CosmosDocuments.CdmElement>();

            CreateMap<ConnectorInfoModel, CosmosDocuments.ConnectorInfo>();

            CreateMap<ConnectorRequestModel, CosmosDocuments.ConnectorRequest > ();
            CreateMap<ConnectorRequestParameterModel, CosmosDocuments.ConnectorRequestParameter > ();

            CreateMap<DataSourceModel, CosmosDocuments.DataSourceDocument>();

            CreateMap<SecurityDefinitionModel, CosmosDocuments.SecurityDefinition>();
               

            CreateMap<SecurityDefinitionDetailsModel, CosmosDocuments.SecurityDefinitionDetails>();
        }

        private static AuthType MapAuthTypeToEnum(string authType)
        {
            switch (authType.ToLower())
            {
                case "oauth2":
                    return AuthType.OAuth2;
                case "transfertoken":
                    return AuthType.TransferToken;
                default:
                    return AuthType.None;

            }
        }
    }
}
