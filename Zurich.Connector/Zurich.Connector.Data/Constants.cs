﻿namespace Zurich.Connector.Data
{
    /// <summary>
    /// Constants
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Supported app codes
        /// </summary>
        public static class AppCodes
        {
            /// <summary>
            /// HighQ code
            /// </summary>
            public const string HighQ = "HighQ";

            /// <summary>
            /// Lexis Nexis UK code
            /// </summary>
            public const string LexisNexisUK = "LexisNexisUK";

            /// <summary>
            /// TTTenantApp code
            /// </summary>
            public const string TTTenantApp = "TTTenantApp";
        }

        /// <summary>
        /// Name of properties defined in the connectors
        /// </summary>
        public static class PropertiesConnectorDefinition
        {
            /// <summary>
            /// Count cdmMapping structured property
            /// </summary>
            public const string Count = "Count";

            /// <summary>
            /// Folders cdmMapping structured property
            /// </summary>
            public const string Folders = "Folders";

            /// <summary>
            /// Documents cdmMapping structured property
            /// </summary>
            public const string Documents = "Documents";
        }

        public static class ContentTypes
        {
            /// <summary>
            /// application/json response type
            /// </summary>
            public const string ApplicationJson = "application/json";

            /// <summary>
            /// text/xml response type
            /// </summary>
            public const string TextXml = "text/xml";
        }
    }

    
}
