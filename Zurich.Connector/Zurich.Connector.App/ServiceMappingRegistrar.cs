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

            CreateMap<FilterList, FilterListModel>();

            CreateMap<CDMMapping, CDMMappingModel>();
            CreateMap<CDMElement, CDMElementModel>();

            CreateMap<ConnectorRequestParameter, ConnectorRequestParameterModel>();
            CreateMap<ConnectorRequestSorting, ConnectorRequestSortingModel>();
            CreateMap<ConnectorRequestSortingProperties, ConnectorRequestSortingPropertiesModel>();

            CreateMap<RegistrationInfo, RegistrationInfoModel>();
            CreateMap<DomainSpecificInformation, DomainSpecificInformationModel>();

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
            CreateMap<RegistrationInfoModel, RegistrationInfo>();
            CreateMap<DomainSpecificInformationModel, DomainSpecificInformation>();

            CreateMap<SecurityDefinitionModel, SecurityDefinition>();


            CreateMap<SecurityDefinitionDetailsModel, SecurityDefinitionDetails>();
        }
    }
}
