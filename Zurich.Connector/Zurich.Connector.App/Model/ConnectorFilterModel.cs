using System;
using System.Collections.Generic;
using System.Text;
using Zurich.Common.Models.Connectors;

namespace Zurich.Connector.Web.Models
{
    /// <summary>
    /// Class with connector information to show to the user
    /// Service model of ConnectorFilterViewModel
    /// </summary>
    public class ConnectorFilterModel
    {
        /// <summary>
        /// EntityType is optional parameter to filter to specific data types of connectors e.g. Document, Search, etc.
        /// </summary>
        public IEnumerable<EntityType> EntityTypes { get; set; }

        /// <summary>
        /// If true - Get only connectors that are registered to a user
        /// </summary>
        public bool IsRegistered { get; set; }

        /// <summary>
        /// RegistrationMode is optional parameter to filter by specific registration mode for the connectors.
        /// </summary>
        public List<RegistrationEntityMode> RegistrationMode { get; set; }

        /// <summary>
        /// DataSource is optional parameter to filter by specific data source.
        /// </summary>
        public List<string> DataSources { get; set; }
    }
}