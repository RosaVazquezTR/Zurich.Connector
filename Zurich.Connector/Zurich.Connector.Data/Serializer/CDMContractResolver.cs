using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;
using Newtonsoft.Json.Serialization;

namespace Zurich.Connector.Data.Serializer
{
    class CDMContractResolver : DefaultContractResolver
    {
        private List<DataMappingProperty> _propertyMap { get; set; }

        public CDMContractResolver(List<DataMappingProperty> propertyMap)
        {
            _propertyMap = propertyMap;
        }

        /// <summary>
        /// Does the mapping from the json properties to the CDM properites
        /// </summary>
        /// <param name="propertyName">This is the CDM object property</param>
        /// <returns>the API property name</returns>
        protected override string ResolvePropertyName(string propertyName)
        {
            var property = _propertyMap.FirstOrDefault(x => x.CDMProperty == propertyName);
            if (!string.IsNullOrEmpty(property?.APIProperty))
            {
                return base.ResolvePropertyName(property.APIProperty);
            }
            // if no property setup try the current name.
            return propertyName;
        }
    }
}
