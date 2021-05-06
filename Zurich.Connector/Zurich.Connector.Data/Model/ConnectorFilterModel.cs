using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Model
{
    /// <summary>
    /// Class for filtering connections
    /// </summary>
    public class ConnectorFilterModel
    {
        /// <summary>
        /// Entity type filters
        /// </summary>
        public List<EntityType> EntityTypes { get; set; }

        /// <summary>
        /// Authorization type filters
        /// </summary>
        public List<AuthType> AuthTypes { get; set; }

        /// <summary>
        /// Data source filtered by app codes
        /// </summary>
        public List<string> DataSources { get; set; }
    }
}
