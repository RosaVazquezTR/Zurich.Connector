using System;
using System.Linq;
using AutoMapper;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Web.Models;

namespace Zurich.Connector.Web
{
	public class MappingRegistrar : Profile
	{
		public MappingRegistrar()
		{

            CreateMap<ConnectorsConfigResponseEntity, ConnectorConfigViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AppCode, opt => opt.MapFrom(src => src.AppCode))
                .ForMember(dest => dest.AuthType, opt => opt.MapFrom(src => src.AuthType))
                .ForMember(dest => dest.CDMData, opt => opt.MapFrom(src => src.CDMData))
                .ForMember(dest => dest.Api, opt => opt.MapFrom(src => src.Api))
                .ForMember(dest => dest.filters, opt => opt.MapFrom(src => src.filters))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<ConnectorModel, ConnectorViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.EntityType, opt => opt.MapFrom(src => src.Info.EntityType))
                .ForMember(dest => dest.DataSource, opt => opt.MapFrom(src => src.DataSource))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<DataSourceModel, DataSourceViewModel>();
            CreateMap<SecurityDefinitionModel, SecurityDefinitionViewModel>();
            CreateMap<SecurityDefinitionDetailsModel, SecurityDefinitionDetailsViewModel>();
        }
    }
}
