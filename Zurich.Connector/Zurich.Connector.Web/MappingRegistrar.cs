using System;
using System.Linq;
using AutoMapper;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Web.Models;

namespace Zurich.Connector.Web
{
	public class MappingRegistrar : Profile
	{
		public MappingRegistrar()
		{
			CreateMap<DataMappingConnection, ConnectorViewModel>()
				.ForMember(dest => dest.AuthType, opt => opt.MapFrom(src => src.Auth.Type))
				.ForMember(dest => dest.DataSource, opt => opt.MapFrom(src => src.AppCode));
            CreateMap<ConnectorsConfigResponseEntity, ConnectorConfigViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AppCode, opt => opt.MapFrom(src => src.AppCode))
                .ForMember(dest => dest.AuthType, opt => opt.MapFrom(src => src.AuthType))
                .ForMember(dest => dest.CDMData, opt => opt.MapFrom(src => src.CDMData))
                .ForMember(dest => dest.Api, opt => opt.MapFrom(src => src.Api))
                .ForMember(dest => dest.filters, opt => opt.MapFrom(src => src.filters))
                .ForAllOtherMembers(opt => opt.Ignore());
		}
	}
}
