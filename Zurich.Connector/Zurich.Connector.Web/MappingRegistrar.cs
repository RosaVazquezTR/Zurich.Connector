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
		}
	}
}
