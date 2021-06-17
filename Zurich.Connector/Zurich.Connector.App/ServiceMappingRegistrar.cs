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

            CreateMap<CosmosDocuments.Structured, Structured>();
            CreateMap<CosmosDocuments.Unstructured, Unstructured>();

            CreateMap<CosmosDocuments.CdmElement, CdmElement>();

            CreateMap<CosmosDocuments.ConnectorInfo, ConnectorInfoModel>();

            CreateMap<CosmosDocuments.ConnectorRequest, ConnectorRequestModel>();
            CreateMap<CosmosDocuments.ConnectorRequestParameter, ConnectorRequestParameterModel>();

            CreateMap<CosmosDocuments.DataSourceDocument, DataSourceModel>();

            CreateMap<CosmosDocuments.SecurityDefinition, SecurityDefinitionModel>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
                .ForMember(dest => dest.TypeEnum, opt => opt.MapFrom(src => MapAuthTypeToEnum(src.type)))
                .ForMember(dest => dest.Flow, opt => opt.MapFrom(src => src.flow))
                .ForMember(dest => dest.DefaultSecurityDefinition, opt => opt.MapFrom(src => src.defaultSecurityDefinition))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<CosmosDocuments.SecurityDefinitionDetails, SecurityDefinitionDetailsModel>();

            /************************************************************************************************************/
            CreateMap<ConnectorModel, CosmosDocuments.ConnectorDocument>();

            CreateMap<CdmMapping, CosmosDocuments.CDMMapping>();

            CreateMap<Structured, CosmosDocuments.Structured>(); 
            CreateMap<Unstructured, CosmosDocuments.Unstructured>();

            CreateMap<CdmElement, CosmosDocuments.CdmElement>();

            CreateMap<ConnectorInfoModel, CosmosDocuments.ConnectorInfo>();

            CreateMap<ConnectorRequestModel, CosmosDocuments.ConnectorRequest > ();
            CreateMap<ConnectorRequestParameterModel, CosmosDocuments.ConnectorRequestParameter > ();

            CreateMap<DataSourceModel, CosmosDocuments.DataSourceDocument>();

            CreateMap<SecurityDefinitionModel, CosmosDocuments.SecurityDefinition>()
                .ForMember(dest => dest.type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.flow, opt => opt.MapFrom(src => src.Flow))
                .ForMember(dest => dest.defaultSecurityDefinition, opt => opt.MapFrom(src => src.DefaultSecurityDefinition))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<SecurityDefinitionDetailsModel, CosmosDocuments.SecurityDefinitionDetails>();
        }

        private static AuthType MapAuthTypeToEnum(string authType)
        {
            switch (authType.ToLower())
            {
                case "oauth2":
                    return AuthType.OAuth;
                case "transfertoken":
                    return AuthType.TransferToken;
                default:
                    return AuthType.None;

            }
        }
    }
}
