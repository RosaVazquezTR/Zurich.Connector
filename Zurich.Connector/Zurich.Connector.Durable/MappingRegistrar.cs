using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Durable.Model;

namespace Zurich.Connector.Durable
{
    public class MappingRegistrar : Profile
    {
        public MappingRegistrar()
        {
            //Mapping for PLtaxonomy
            CreateMap<PracticeAreaList, TaxonomyOptions>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value));
            CreateMap<ServiceDef, TaxonomyOptions>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.Value));
            CreateMap<TaxonomyOptions, FilterList>();
        }
    }
}
