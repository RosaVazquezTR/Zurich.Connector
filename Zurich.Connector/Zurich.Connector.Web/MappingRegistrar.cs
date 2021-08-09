﻿using System;
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

            CreateMap<ConnectorModel, ConnectorViewModel>();
            CreateMap<ConnectorInfoModel, ConnectorDetailsInfoViewModel>();
            CreateMap<DataSourceModel, DataSourceViewModel>();
            CreateMap<ConnectorRequestModel, ConnectorRequestViewModel>();
            CreateMap<ConnectorRequestParameterModel, ConnectorRequestParameterViewModel>();
            CreateMap<ConnectorRequestSortingModel, ConnectorRequestSortingViewModel>();
            CreateMap<ConnectorRequestSortingPropertiesModel, ConnectorRequestSortingPropertiesViewModel>();

            CreateMap<ConnectorResponseModel, ConnectorResponseViewModel>();
            CreateMap<ConnectorReponseSchemaModel, ConnectorReponseSchemaViewModel>();
            CreateMap<ConnectorReponsePropertiesModel, ConnectorReponsePropertiesViewModel>();

            CreateMap<ConnectorFilterModel, FilterViewModel>();

            CreateMap<CDMMappingModel, CDMMappingViewModel>();
            CreateMap<CDMElementModel, CDMElementViewModel>();

            CreateMap<ConnectorModel, ConnectorListViewModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.EntityType, opt => opt.MapFrom(src => src.Info.EntityType))
                .ForMember(dest => dest.DataSource, opt => opt.MapFrom(src => src.DataSource))
                .ForAllOtherMembers(opt => opt.Ignore());

            
            CreateMap<DataSourceModel, DataSourceViewModel>();
            CreateMap<SecurityDefinitionModel, SecurityDefinitionViewModel>();
            CreateMap<SecurityDefinitionDetailsModel, SecurityDefinitionDetailsViewModel>();

            CreateMap<ConnectorRegistration, ConnectorRegistrationViewModel>()
                .ForMember(dest => dest.Connectorid, opt => opt.MapFrom(src => src.ConnectorId))
                .ForMember(dest => dest.Userid, opt => opt.MapFrom(src => src.UserId))
                .ForMember(dest => dest.Tenantid, opt => opt.MapFrom(src => src.TenantId))
                .ForMember(dest => dest.DataSourceid, opt => opt.MapFrom(src => src.DataSourceId))
                .ForMember(dest => dest.AppName, opt => opt.MapFrom(src => src.AppName))
                .ForMember(dest => dest.RegistrationStatus, opt => opt.MapFrom(src => src.RegistrationStatus))
                .ForAllOtherMembers(opt => opt.Ignore());

        }
    }
}
