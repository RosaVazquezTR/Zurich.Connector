using AutoMapper;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Models.CommonDataModels;

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
            CreateMap<ConnectorAdvancedSyntax, ConnectorAdvancedSyntaxModel>();
            CreateMap<AdvancedSyntaxOperator, AdvancedSyntaxOperatorModel>();

            CreateMap<RegistrationInfo, RegistrationInfoModel>();
            CreateMap<DomainSpecificInformation, DomainSpecificInformationModel>();

            CreateMap<ConnectorRegistrationDocument, ConnectorRegistration>()
                .ForMember(dest => dest.ConnectorId, opt => opt.MapFrom(src => src.ConnectorId))
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.TenantId, opt => opt.MapFrom(src => src.TenantId))
                .ForMember(dest => dest.AppName, opt => opt.MapFrom(src => src.AppName))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            CreateMap<ConnectorResponse, ConnectorResponseModel>();
            CreateMap<ConnectorReponseSchema, ConnectorReponseSchemaModel>();
            CreateMap<ConnectorReponseProperties, ConnectorReponsePropertiesModel>();
            CreateMap<ConnectorFacet, ConnectorsFacetsModel>();
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
            CreateMap<ConnectorAdvancedSyntaxModel, ConnectorAdvancedSyntax>();
            CreateMap<AdvancedSyntaxOperatorModel, AdvancedSyntaxOperator>();

            CreateMap<ConnectorResponseModel, ConnectorResponse>();
            CreateMap<ConnectorReponseSchemaModel, ConnectorReponseSchema>();
            CreateMap<ConnectorReponsePropertiesModel, ConnectorReponseProperties>();
            CreateMap<ConnectorsFacetsModel, ConnectorFacet>();
            CreateMap<ConnectorsFiltersModel, ConnectorFilter>();
            CreateMap<FilterListModel, FilterList>();

            CreateMap<DataSourceModel, DataSourceDocument>();
            CreateMap<RegistrationInfoModel, RegistrationInfo>();
            CreateMap<DomainSpecificInformationModel, DomainSpecificInformation>();

            CreateMap<SecurityDefinitionModel, SecurityDefinition>();


            CreateMap<SecurityDefinitionDetailsModel, SecurityDefinitionDetails>();
            CreateMap<AppToken, OAuthAPITokenResponse>()
                .ForMember(dest => dest.AccessToken, opt => opt.MapFrom(src => src.access_token))
                .ForMember(dest => dest.TokenType, opt => opt.MapFrom(src => src.token_type))
                .ForMember(dest => dest.ExpiresOn, opt => opt.MapFrom(src => src.expires_on));
            CreateMap<OAuthAPITokenResponse, Token>(MemberList.Destination);
        }
    }
}
