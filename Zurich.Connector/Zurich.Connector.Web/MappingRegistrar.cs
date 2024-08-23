using System;
using System.Linq;
using AutoMapper;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Web.Models;
using Zurich.Connector.Web.Enum;
namespace Zurich.Connector.Web
{
    public class MappingRegistrar : Profile
    {
        public MappingRegistrar()
        {

            CreateMap<ConnectorModel, ConnectorViewModel>()
                .ForMember(dest => dest.EntityType, opt => opt.MapFrom(src => src.Info.EntityType))
                .ForMember(dest => dest.Sort, opt => opt.MapFrom(src => src.Request.Sorting.Properties.Select(x => SortType.Parse(typeof(SortType), x.Name, true))));

            // Mapping ResponseContentType to JSON by default, if response Type value doesn't exists in ResponseContentType enum
            CreateMap<ConnectorResponse, ConnectorResponseModel>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(
                src => System.Enum.IsDefined(typeof(ResponseContentType), src.Type) ? System.Enum.Parse(typeof(ResponseContentType), src.Type.ToString()) : ResponseContentType.JSON));

            CreateMap<ConnectorInfoModel, ConnectorDetailsInfoViewModel>();
            CreateMap<ConnectorRequestModel, ConnectorRequestViewModel>();
            CreateMap<ConnectorRequestParameterModel, ConnectorRequestParameterViewModel>();
            CreateMap<ConnectorRequestSortingModel, ConnectorRequestSortingViewModel>();
            CreateMap<ConnectorRequestSortingPropertiesModel, ConnectorRequestSortingPropertiesViewModel>();

            CreateMap<ConnectorResponseModel, ConnectorResponseViewModel>();
            CreateMap<ConnectorReponseSchemaModel, ConnectorReponseSchemaViewModel>();
            CreateMap<ConnectorReponsePropertiesModel, ConnectorReponsePropertiesViewModel>();

            CreateMap<ConnectorsFiltersModel, FilterViewModel>();
            CreateMap<FilterListModel, FilterListViewModel>();

            CreateMap<CDMMappingModel, CDMMappingViewModel>();
            CreateMap<CDMElementModel, CDMElementViewModel>();

            CreateMap<ConnectorModel, ConnectorListViewModel>()
                .ForMember(dest => dest.ExternalUserId, opt => opt.MapFrom(src => src.Info.ExternalUserId))
                .ForMember(dest => dest.AcceptsSearchWildCard, opt => opt.MapFrom(src => src.Info.AcceptsSearchWildCard))
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.EntityType, opt => opt.MapFrom(src => src.Info.EntityType))
                .ForMember(dest => dest.DataSource, opt => opt.MapFrom(src => src.DataSource))
                .ForMember(dest => dest.Filters, opt => opt.MapFrom(src => src.Filters))
                .ForMember(dest => dest.RegistrationStatus, opt => opt.MapFrom(src => src.RegistrationStatus))
                .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.Alias))
                .ForMember(dest => dest.Sort, opt => opt.MapFrom(src => src.Request.Sorting.Properties.Select(x => SortType.Parse(typeof(SortType), x.Name, true))));


            CreateMap<DataSourceModel, DataSourceViewModel>();
            CreateMap<RegistrationInfoModel, RegistrationInfoViewModel>();
            CreateMap<DomainSpecificInformationModel, DomainSpecificInformationViewModel>();
            CreateMap<SecurityDefinitionModel, SecurityDefinitionViewModel>();
            CreateMap<SecurityDefinitionDetailsModel, SecurityDefinitionDetailsViewModel>();

            CreateMap<ConnectorRegistration, ConnectorRegistrationViewModel>()
                .ForMember(dest => dest.Connectorid, opt => opt.MapFrom(src => src.ConnectorId))
                .ForMember(dest => dest.Userid, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Tenantid, opt => opt.MapFrom(src => src.TenantId))
                .ForMember(dest => dest.AppName, opt => opt.MapFrom(src => src.AppName));

            CreateMap<DataSourceRegistration, DataSourceRegistrationResponseViewModel>()
                .ForMember(dest => dest.AuthorizeUrl, opt => opt.MapFrom(src => src.AuthorizeUrl))
                .ForMember(dest => dest.Registered, opt => opt.MapFrom(src => src.Registered));

            CreateMap<ConnectorFilterViewModel, Zurich.Connector.Web.Models.ConnectorFilterModel>();


        }
    }
}
