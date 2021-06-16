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
            CreateMap<CosmosDocuments.ConnectorDocument, ConnectorModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Info, opt => opt.MapFrom(src => src.info))
                .ForMember(dest => dest.Request, opt => opt.MapFrom(src => src.request))
                .ForMember(dest => dest.CdmMapping, opt => opt.MapFrom(src => src.cdmMapping))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<CosmosDocuments.CDMMapping, CdmMapping>()
                .ForMember(dest => dest.Structured, opt => opt.MapFrom(src => src.structured))
                .ForMember(dest => dest.Unstructured, opt => opt.MapFrom(src => src.unstructured))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<CosmosDocuments.Structured, Structured>()
               .ForMember(dest => dest.CdmElement, opt => opt.MapFrom(src => src.CdmElement))
               .ForAllOtherMembers(opt => opt.Ignore());
            CreateMap<CosmosDocuments.Unstructured, Unstructured>()
                    .ForMember(dest => dest.DataElement, opt => opt.MapFrom(src => src.DataElement))
                    .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<CosmosDocuments.CdmElement, CdmElement>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ResponseElement, opt => opt.MapFrom(src => src.ResponseElement))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest._Comment, opt => opt.MapFrom(src => src._Comment))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<CosmosDocuments.ConnectorInfo, ConnectorInfoModel>();

            CreateMap<CosmosDocuments.ConnectorRequest, ConnectorRequestModel>();
            CreateMap<CosmosDocuments.ConnectorRequestParameter, ConnectorRequestParameterModel>();

            CreateMap<CosmosDocuments.DataSourceDocument, DataSourceModel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.description))
                .ForMember(dest => dest.InfoUrl, opt => opt.MapFrom(src => src.infoUrl))
                .ForMember(dest => dest.ExtraRequestContext, opt => opt.MapFrom(src => src.extraRequestContext))
                .ForMember(dest => dest.SecurityDefinition, opt => opt.MapFrom(src => src.securityDefinition))
                .ForMember(dest => dest.AppCode, opt => opt.MapFrom(src => src.AppCode))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<CosmosDocuments.SecurityDefinition, SecurityDefinitionModel>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
                .ForMember(dest => dest.TypeEnum, opt => opt.MapFrom(src => MapAuthTypeToEnum(src.type)))
                .ForMember(dest => dest.Flow, opt => opt.MapFrom(src => src.flow))
                .ForMember(dest => dest.DefaultSecurityDefinition, opt => opt.MapFrom(src => src.defaultSecurityDefinition))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<CosmosDocuments.SecurityDefinitionDetails, SecurityDefinitionDetailsModel>()
                .ForMember(dest => dest.AuthorizationURL, opt => opt.MapFrom(src => src.authorizationURL))
                .ForMember(dest => dest.AuthorizationPath, opt => opt.MapFrom(src => src.authorizationPath))
                .ForMember(dest => dest.TokenURL, opt => opt.MapFrom(src => src.tokenURL))
                .ForMember(dest => dest.TokenPath, opt => opt.MapFrom(src => src.tokenPath))
                .ForMember(dest => dest.KeyVaultClientId, opt => opt.MapFrom(src => src.keyVaultClientId))
                .ForMember(dest => dest.KeyVaultSecret, opt => opt.MapFrom(src => src.keyVaultSecret))
                .ForAllOtherMembers(opt => opt.Ignore());

            /*******************************************************************************************/
            CreateMap<ConnectorModel, ConnectorModelEntity>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Info, opt => opt.MapFrom(src => src.Info))
                .ForMember(dest => dest.Request, opt => opt.MapFrom(src => src.Request))
                .ForMember(dest => dest.CdmMappingEntity, opt => opt.MapFrom(src => src.CdmMapping))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<ConnectorInfoModel, ConnectorInfoModelEntity>();

            CreateMap<ConnectorRequestModel, ConnectorRequestModelEntity>();
            CreateMap<ConnectorRequestParameterModel, ConnectorRequestParameterModelEntity>();

            CreateMap<DataSourceModel, DataSourceModelEntity>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.AppCode, opt => opt.MapFrom(src => src.AppCode))
                .ForMember(dest => dest.InfoUrl, opt => opt.MapFrom(src => src.InfoUrl))
                .ForMember(dest => dest.ExtraRequestContext, opt => opt.MapFrom(src => src.ExtraRequestContext))
                .ForMember(dest => dest.SecurityDefinition, opt => opt.MapFrom(src => src.SecurityDefinition))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<SecurityDefinitionModel, SecurityDefinitionModelEntity>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.TypeEnum, opt => opt.MapFrom(src => MapAuthTypeToEnum(src.Type)))
                .ForMember(dest => dest.Flow, opt => opt.MapFrom(src => src.Flow))
                .ForMember(dest => dest.DefaultSecurityDefinition, opt => opt.MapFrom(src => src.DefaultSecurityDefinition))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<SecurityDefinitionDetailsModel, SecurityDefinitionDetailsModelEntity>()
                .ForMember(dest => dest.AuthorizationURL, opt => opt.MapFrom(src => src.AuthorizationURL))
                .ForMember(dest => dest.AuthorizationPath, opt => opt.MapFrom(src => src.AuthorizationPath))
                .ForMember(dest => dest.TokenURL, opt => opt.MapFrom(src => src.TokenURL))
                .ForMember(dest => dest.TokenPath, opt => opt.MapFrom(src => src.TokenPath))
                .ForMember(dest => dest.KeyVaultClientId, opt => opt.MapFrom(src => src.KeyVaultClientId))
                .ForMember(dest => dest.KeyVaultSecret, opt => opt.MapFrom(src => src.KeyVaultSecret))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<CdmMapping, CdmMappingEntity>()
                .ForMember(dest => dest.StructuredEntity, opt => opt.MapFrom(src => src.Structured))
                .ForMember(dest => dest.UnstructuredEntity, opt => opt.MapFrom(src => src.Unstructured))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<Structured, StructuredEntity>()
                   .ForMember(dest => dest.CdmElementEntity, opt => opt.MapFrom(src => src.CdmElement))
                   .ForAllOtherMembers(opt => opt.Ignore());
            CreateMap<Unstructured, UnstructuredEntity>()
                    .ForMember(dest => dest.DataElementEntity, opt => opt.MapFrom(src => src.DataElement))
                    .ForAllOtherMembers(opt => opt.Ignore());
            
            CreateMap<CdmElement, CdmElementEntity>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ResponseElement, opt => opt.MapFrom(src => src.ResponseElement))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest._Comment, opt => opt.MapFrom(src => src._Comment))
                .ForAllOtherMembers(opt => opt.Ignore());

            /***********************************************************************************************************/
            CreateMap<CosmosDocuments.ConnectorDocument, ConnectorModelEntity>()
                   .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                   .ForMember(dest => dest.Info, opt => opt.MapFrom(src => src.info))
                   .ForMember(dest => dest.Request, opt => opt.MapFrom(src => src.request))
                   .ForMember(dest => dest.CdmMappingEntity, opt => opt.MapFrom(src => src.cdmMapping))
                   .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<CosmosDocuments.CDMMapping, CdmMappingEntity>()
                    .ForMember(dest => dest.StructuredEntity, opt => opt.MapFrom(src => src.structured))
                    .ForMember(dest => dest.UnstructuredEntity, opt => opt.MapFrom(src => src.unstructured))
                    .ForAllOtherMembers(opt => opt.Ignore());


            CreateMap<CosmosDocuments.Structured, StructuredEntity>()
                    .ForMember(dest => dest.CdmElementEntity, opt => opt.MapFrom(src => src.CdmElement))
                    .ForAllOtherMembers(opt => opt.Ignore()); 
            CreateMap<CosmosDocuments.Unstructured, UnstructuredEntity>()
                    .ForMember(dest => dest.DataElementEntity, opt => opt.MapFrom(src => src.DataElement))
                    .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<CosmosDocuments.CdmElement, CdmElementEntity>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.ResponseElement, opt => opt.MapFrom(src => src.ResponseElement))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest._Comment, opt => opt.MapFrom(src => src._Comment))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<CosmosDocuments.ConnectorInfo, ConnectorInfoModelEntity>();

            CreateMap<CosmosDocuments.ConnectorRequest, ConnectorRequestModelEntity>();
            CreateMap<CosmosDocuments.ConnectorRequestParameter, ConnectorRequestParameterModelEntity>();

            CreateMap<CosmosDocuments.DataSourceDocument, DataSourceModelEntity>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.description))
                .ForMember(dest => dest.InfoUrl, opt => opt.MapFrom(src => src.infoUrl))
                .ForMember(dest => dest.ExtraRequestContext, opt => opt.MapFrom(src => src.extraRequestContext))
                .ForMember(dest => dest.SecurityDefinition, opt => opt.MapFrom(src => src.securityDefinition))
                .ForMember(dest => dest.AppCode, opt => opt.MapFrom(src => src.AppCode))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<CosmosDocuments.SecurityDefinition, SecurityDefinitionModelEntity>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
                .ForMember(dest => dest.TypeEnum, opt => opt.MapFrom(src => MapAuthTypeToEnum(src.type)))
                .ForMember(dest => dest.Flow, opt => opt.MapFrom(src => src.flow))
                .ForMember(dest => dest.DefaultSecurityDefinition, opt => opt.MapFrom(src => src.defaultSecurityDefinition))
                .ForAllOtherMembers(opt => opt.Ignore());

            CreateMap<CosmosDocuments.SecurityDefinitionDetails, SecurityDefinitionDetailsModelEntity>()
                .ForMember(dest => dest.AuthorizationURL, opt => opt.MapFrom(src => src.authorizationURL))
                .ForMember(dest => dest.AuthorizationPath, opt => opt.MapFrom(src => src.authorizationPath))
                .ForMember(dest => dest.TokenURL, opt => opt.MapFrom(src => src.tokenURL))
                .ForMember(dest => dest.TokenPath, opt => opt.MapFrom(src => src.tokenPath))
                .ForMember(dest => dest.KeyVaultClientId, opt => opt.MapFrom(src => src.keyVaultClientId))
                .ForMember(dest => dest.KeyVaultSecret, opt => opt.MapFrom(src => src.keyVaultSecret))
                .ForAllOtherMembers(opt => opt.Ignore());
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
