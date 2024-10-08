﻿using System.Collections.Generic;
using Zurich.Connector.App.Enum;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.App.Model
{
    /// <summary>
    /// Store connector details
    /// </summary>
    public class ConnectorModel
    {
        /// <summary>
        /// Connector ID
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Connector Alias in the format of [appcode].[entitytype].[subtype?] for e.g. msgraph.document.recent, plcca.search
        /// </summary>
        public string Alias { get; set; }
        /// <summary>
        /// Connector Info like title, description etc.
        /// </summary>
        public ConnectorInfoModel Info { get; set; }
        /// <summary>
        /// Store the data source details.
        /// </summary>
        public DataSourceModel DataSource { get; set; }
        /// <summary>
        /// Request details
        /// </summary>
        public ConnectorRequestModel Request { get; set; }
        /// <summary>
        /// Response details
        /// </summary>
        public ConnectorResponseModel Response { get; set; }
        /// <summary>
        /// Facets details
        /// </summary>
        public ConnectorsFacetsModel FacetsInformation { get; set; }
        /// <summary>
        /// Ontologies details
        /// </summary>
        public ConnectorsOntologiesModel OntologiesInformation { get; set; }
        /// <summary>
        /// Filter details
        /// </summary>
        public IEnumerable<ConnectorsFiltersModel> Filters { get; set; }
        /// <summary>
        /// Pagination properties.
        /// </summary>
        public PaginationModel Pagination { get; set; }
        /// <summary>
        /// CDM Mapping of properties
        /// </summary>
        public CDMMappingModel CDMMapping { get; set; }
        /// <summary>
        /// Host name
        /// </summary>
        public string HostName { get; set; }
        /// <summary>
        /// Result Location
        /// </summary>
        public string ResultLocation { get; set; }

        /// <summary>
        /// Registration status
        /// </summary>
        public RegistrationStatus RegistrationStatus { get; set; }
        /// <summary>
        /// Determines whether to show pre release connectors 
        /// </summary>
        public bool PreRelease { get; set; }
        /// <summary>
        /// Syntax for advanced search operators if supported
        /// </summary>
        public ConnectorAdvancedSyntaxModel AdvancedSearchSyntax { get; set; }
        /// <summary>
        /// Determines if the connector requires to call oauth to get an authentication token
        /// </summary>
        public bool BasicAuthentication { get; set; }

    }

    /// <summary>
    /// Store connector info
    /// </summary>
    public class ConnectorInfoModel
    {
        /// <summary>
        /// Connector title
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Connector description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Entity type like Document, Search etc.
        /// </summary>
        public ConnectorEntityType EntityType { get; set; }
        /// <summary>
        /// Data Source ID
        /// </summary>
        public string DataSourceId { get; set; }
        /// <summary>
        /// Version
        /// </summary>
        public string Version { get; set; }
        /// <summary>
        /// Gets or sets the subtype
        /// </summary>
        public string SubType { get; set; }
        /// <summary>
        /// Flag to indicate if a dynamic filter is available
        /// </summary>
        public bool? IsDynamicFilter { get; set; }
        /// <summary>
        /// Sets or gets the responsable connector to download documents
        /// </summary>
        public string DownloadConnector { get; set; }
        /// <summary>
        /// Determines if a connector accepts or not the * search wildcard
        /// </summary>
        public bool? AcceptsSearchWildCard { get; set; }
        /// <summary>
        /// External User ID for the PL Docs connector
        /// </summary>
        public string ExternalUserId { get; set; }
    }

    /// <summary>
    /// Pagination properties.
    /// </summary>
    public class PaginationModel
    {
        /// <summary>
        /// Indicates if pagination available for this connector.
        /// </summary>
        public bool Available { get; set; }
        /// <summary>
        /// Pagination Type.
        /// </summary>
        public string PaginationType { get; set; }
        /// <summary>
        /// Offset base. It will be true if the offset base is 0.
        /// </summary>
        public bool? IsZeroBasedOffset { get; set; }
    }
}
