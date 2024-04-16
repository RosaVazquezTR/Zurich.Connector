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
        /// <summary>
        /// CMD mapping of properties.
        /// </summary>
        public List<CDMElement> structured { get; set; }
        /// <summary>
        /// Place holder to store properties which is not part of the structured definition.
        /// </summary>
        public List<CDMElement> unstructured { get; set; }

    }

    /// <summary>
    /// Store the CDM mapping of individual elements
    /// </summary>
    public class CDMElement
    {
        /// <summary>
        /// Name of the property
        /// </summary>
        public string name { get; set; }
        /// <summary>
        /// Type of the property
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// Name of the response element
        /// </summary>
        public string responseElement { get; set; }
        /// <summary>
        /// Format to be used
        /// </summary>
        public string format { get; set; }
        /// <summary>
        /// Property to validate if is an inner property
        /// </summary>
        public bool isInner { get; set; } = true;
    }
}
