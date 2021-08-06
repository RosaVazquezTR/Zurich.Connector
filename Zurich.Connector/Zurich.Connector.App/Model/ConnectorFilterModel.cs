﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.App.Model
{
    /// <summary>
    /// Class to filter connectors
    /// </summary>
    public class ConnectorFilterModel
    {
        /// <summary>
        /// EntityType is optional parameter to filter to specific data types of connectors e.g. Document, Search, etc.
        /// </summary>
        public List<EntityType> EntityTypes { get; set; }

        /// <summary>
        /// RegistrationMode is optional parameter to filter by specific registration mode for the connectors.
        /// e.g. Registered would return all connectors that are registered for the user.
        /// </summary>
        // TODO - Enable registration mode filter
        //public List<RegistrationEntityMode> RegistrationModes { get; set; }

        /// <summary>
        /// DataSource is optional parameter to filter by specific data source.
        /// </summary>
        public List<string> DataSources { get; set; }
    }
}
