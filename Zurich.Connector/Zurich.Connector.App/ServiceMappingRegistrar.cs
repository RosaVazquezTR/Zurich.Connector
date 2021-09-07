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

            CreateMap<PaginationInfo, PaginationModel>();
            CreateMap<ConnectorRequest, ConnectorRequestModel>();
            CreateMap<ConnectorResponse, ConnectorResponseModel>();
            CreateMap<ConnectorReponseSchema, ConnectorReponseSchemaModel>();
            CreateMap<ConnectorReponseProperties, ConnectorReponsePropertiesModel> ();

            CreateMap<ConnectorFilter, ConnectorsFiltersModel>();
            CreateMap<FilterList, FilterListModel>();
            CreateMap<FilterListModel, FilterList>();

            CreateMap<CDMMapping, CDMMappingModel>();
            CreateMap<CDMElement, CDMElementModel>();

            CreateMap<ConnectorRequestParameter, ConnectorRequestParameterModel>();
            CreateMap<ConnectorRequestSorting, ConnectorRequestSortingModel>();
            CreateMap<ConnectorRequestSortingProperties, ConnectorRequestSortingPropertiesModel>();

            CreateMap<DataSourceDocument, DataSourceModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.description))
                .ForMember(dest => dest.InfoUrl, opt => opt.MapFrom(src => src.infoUrl))
                .ForMember(dest => dest.ExtraRequestContext, opt => opt.MapFrom(src => src.extraRequestContext))
                .ForMember(dest => dest.SecurityDefinition, opt => opt.MapFrom(src => src.securityDefinition))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<SecurityDefinition, SecurityDefinitionModel>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
                .ForMember(dest => dest.Flow, opt => opt.MapFrom(src => src.flow))
                .ForMember(dest => dest.DefaultSecurityDefinition, opt => opt.MapFrom(src => src.defaultSecurityDefinition))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<SecurityDefinitionDetails, SecurityDefinitionDetailsModel>()
                .ForMember(dest => dest.AuthorizationURL, opt => opt.MapFrom(src => src.authorizationURL))
                .ForMember(dest => dest.AuthorizationPath, opt => opt.MapFrom(src => src.authorizationPath))
                .ForMember(dest => dest.TokenURL, opt => opt.MapFrom(src => src.tokenURL))
                .ForMember(dest => dest.TokenPath, opt => opt.MapFrom(src => src.tokenPath))
                .ForMember(dest => dest.KeyVaultClientId, opt => opt.MapFrom(src => src.keyVaultClientId))
                .ForMember(dest => dest.KeyVaultSecret, opt => opt.MapFrom(src => src.keyVaultSecret))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<ConnectorRegistrationDocument, ConnectorRegistration>()
                .ForMember(dest => dest.ConnectorId, opt => opt.MapFrom(src => src.ConnectorId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId))
                .ForMember(dest => dest.AppName, opt => opt.MapFrom(src => src.AppName))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<ConnectorResponse, ConnectorResponseModel>();
            CreateMap<ConnectorReponseSchema, ConnectorReponseSchemaModel>();
            CreateMap<ConnectorReponseProperties, ConnectorReponsePropertiesModel>();
            CreateMap<ConnectorFilter, ConnectorsFiltersModel>();

            CreateMap<DataSourceDocument, DataSourceModel>();

            CreateMap<SecurityDefinition, SecurityDefinitionModel>();

            CreateMap<SecurityDefinitionDetails, SecurityDefinitionDetailsModel>();

            /************************************************************************************************************/
            CreateMap<ConnectorModel, ConnectorDocument>();

            CreateMap<CDMMappingModel, CDMMapping>();

            CreateMap<CDMElementModel, CDMElement>();

            CreateMap<ConnectorInfoModel, ConnectorInfo>();

            CreateMap<PaginationModel, PaginationInfo>();
            CreateMap<ConnectorRequestModel,ConnectorRequest>();
            CreateMap<ConnectorRequestParameterModel, ConnectorRequestParameter>();
            CreateMap<ConnectorRequestSortingModel, ConnectorRequestSorting>();
            CreateMap<ConnectorRequestSortingPropertiesModel, ConnectorRequestSortingProperties>();

            CreateMap<ConnectorResponseModel, ConnectorResponse>();
            CreateMap<ConnectorReponseSchemaModel, ConnectorReponseSchema>();
            CreateMap<ConnectorReponsePropertiesModel, ConnectorReponseProperties>();
            CreateMap<ConnectorsFiltersModel, ConnectorFilter>();
            CreateMap<FilterListModel, FilterList>();

            CreateMap<DataSourceModel, DataSourceDocument>();

            CreateMap<SecurityDefinitionModel, SecurityDefinition>();


            CreateMap<SecurityDefinitionDetailsModel, SecurityDefinitionDetails>();
        }
    }
}
