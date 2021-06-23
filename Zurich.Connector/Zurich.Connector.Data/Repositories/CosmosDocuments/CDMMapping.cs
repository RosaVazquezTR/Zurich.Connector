using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Repositories.CosmosDocuments
{
    /// <summary>
    /// Store CDM Mapping of properties
    /// </summary>
    public class CDMMapping
    {
        public List<CdmElement> structured { get; set; }
        public List<CdmElement> unstructured { get; set; }

    }

    public class CdmElement
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Type of the property
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// Name of the response element
        /// </summary>
        public string ResponseElement { get; set; }
        /// <summary>
        /// Comment
        /// </summary>
        public string Comment { get; set; }
    }
 
}
