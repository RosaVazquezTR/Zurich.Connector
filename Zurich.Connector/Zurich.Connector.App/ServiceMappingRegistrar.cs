using AutoMapper;
using System;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
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
            CreateMap<ConnectorDocument, ConnectorModel>();

            CreateMap<ConnectorInfo, ConnectorInfoModel>();

            CreateMap<ConnectorRequest, ConnectorRequestModel>();
            CreateMap<ConnectorResponse, ConnectorResponseModel>();
            CreateMap<ConnectorReponseSchema, ConnectorReponseSchemaModel>();
            CreateMap<ConnectorReponseProperties, ConnectorReponsePropertiesModel> ();

            CreateMap<ConnectorFilter, ConnectorFilterModel>();

            CreateMap<CDMMapping, CDMMappingModel>();
            CreateMap<CDMElement, CDMElementModel>();

            CreateMap<ConnectorRequestParameter, ConnectorRequestParameterModel>();

            CreateMap<ConnectorResponse, ConnectorResponseModel>();
            CreateMap<ConnectorReponseSchema, ConnectorReponseSchemaModel>();
            CreateMap<ConnectorReponseProperties, ConnectorReponsePropertiesModel>();
            CreateMap<ConnectorFilter, ConnectorFilterModel>();

            CreateMap<DataSourceDocument, DataSourceModel>();

            CreateMap<SecurityDefinition, SecurityDefinitionModel>();

            CreateMap<SecurityDefinitionDetails, SecurityDefinitionDetailsModel>();

            /************************************************************************************************************/
            CreateMap<ConnectorModel, ConnectorDocument>();

            CreateMap<CDMMappingModel, CDMMapping>();

            CreateMap<CDMElementModel, CDMElement>();

            CreateMap<ConnectorInfoModel, ConnectorInfo>();

            CreateMap<ConnectorRequestModel,ConnectorRequest>();
            CreateMap<ConnectorRequestParameterModel, ConnectorRequestParameter>();

            CreateMap<ConnectorResponseModel, ConnectorResponse>();
            CreateMap<ConnectorReponseSchemaModel, ConnectorReponseSchema>();
            CreateMap<ConnectorReponsePropertiesModel, ConnectorReponseProperties>();
            CreateMap<ConnectorFilterModel, ConnectorFilter>();

            CreateMap<DataSourceModel, DataSourceDocument>();

            CreateMap<SecurityDefinitionModel, SecurityDefinition>();


            CreateMap<SecurityDefinitionDetailsModel, SecurityDefinitionDetails>();
        }
    }
}
