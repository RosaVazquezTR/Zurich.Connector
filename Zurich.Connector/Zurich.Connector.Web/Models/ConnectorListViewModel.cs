﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Zurich.Connector.App.Enum;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Web.Enum;
namespace Zurich.Connector.Web.Models
{
    /// <summary>
    /// Class with only connection information shown to user
    /// </summary>
    public class ConnectorListViewModel
    {
        /// <summary>
        /// The connector id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The connector alias
        /// </summary>
        public string Alias { get; set; }

        /// <summary>
        /// The type of connector
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Data source for connector.
        /// </summary>
        public DataSourceViewModel DataSource { get; set; }

        /// <summary>
        /// Filter details
        /// </summary>
        public IEnumerable<FilterViewModel> Filters { get; set; }

        /// <summary>
        /// Time of creation
        /// </summary>
        public DateTime? CreatedTime { get; set; }

        /// <summary>
        /// Domain of the api endpoint
        /// </summary>
        public string? Domain { get; set; }

        /// <summary>
        /// Registration status
        /// </summary>
        public RegistrationStatus RegistrationStatus { get; set; }

        /// <summary>
        /// Sort type options
        /// </summary>
        public IEnumerable<SortType> Sort { get; set; }

        /// <summary>
        /// Determines if a connector accepts or not the * search wildcard
        /// </summary>
        public bool? AcceptsSearchWildCard { get; set; }
        /// <summary>
        /// External User ID for the PL Docs connector
        /// </summary>
        public string ExternalUserId { get; set; }
    }
}
