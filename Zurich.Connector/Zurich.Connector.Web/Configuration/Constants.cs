namespace Zurich.Connector.Web.Configuration
{
    /// <summary>
    /// Represents a container class for configuration keys
    /// </summary>
    public static class Constants
    {
        public const string PublicBasePath = "PublicBasePath";
    }

    /// <summary>
    /// Represents the supported token types for the extension grant flow
    /// </summary>
    public static class SupportedTokenTypes
    {
        public const string AccessTokenJwt = "at+jwt";
    }

    /// <summary>
    /// Represents the supported CORS policy names
    /// </summary>
    public static class CORSPolicies
    {
        public const string DefaultPolicy = "MainCORS";
    }

    /// <summary>
    /// Represents the supported scopes by the Connector API
    /// </summary>
    public static class LegalPlatformConnectorsScopes
    {
        // TODO: Add Connector specific scopes to Identity Server. Shouldn't use the legalhome ones
        public const string Full = "connectors.full";
    }

    /// <summary>
    /// Represents the Policies for the API Endpoints
    /// </summary>
    public static class Policies
    {
        /// <summary>
        /// Policy for Service Connectors
        /// </summary>
        public const string ServiceConnectorPolicy = "ServiceConnectorPolicy";
    }
}
