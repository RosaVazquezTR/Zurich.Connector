namespace Zurich.Connector.Data
{

    /// <summary>
    /// Represents a container class for CDM structured property names
    /// </summary>
    public static class StructuredCDMProperties
    {
        public const string Title = "Title";
        public const string WebUrl = "WebUrl";
        public const string EntityId = "Id";
        public const string Type = "Type";
        public const string DownloadUrl = "DownloadUrl";
        public const string DesktopUrl = "DesktopUrl";
        public const string Snippet = "Snippet";
        public const string AdditionalProperties = "AdditionalProperties";
        public const string ItemsCount = "Count";
    }

    /// <summary>
    /// Represents a container class for CDM unstructured property names
    /// </summary>
    public static class UnstructuredCDMProperties
    {
        public const string Extension = "extension";
        public const string ListItemUniqueId = "listItemUniqueId";
        public const string Id = "id";
    }

    /// <summary>
    /// Represents a container class for known data source names
    /// </summary>
    public static class KnownDataSources
    {
        public const string iManage = "iManage";
        public const string iManageServiceApp = "iManageServiceApp";
        public const string practicalLawConnect = "PracticalLawConnect";
        public const string practicalLawConnectOnePass = "CBTPRACPT";
        public const string plcUS = "PLCUS";
        public const string plcUK = "PLCUK";
        public const string plcCA = "PLCCA";
        public const string practicalLawConnectSearch = "PracticalLawConnect-Search";
        public const string practicalLawConnectSearchOnePass = "CBTPRACPT-Search";
        public const string plcUSSearch = "PLCUS-Search";
        public const string plcUKSearch = "PLCUK-Search";
        public const string plcCASearch = "PLCCA-Search";
        public const string msGraph = "MsGraph";
        public const string westLawUK = "WLUK";

    }
    public static class DataConstants
    {
        public const string LegalHomeScope = "legalhome.full";
        public const string OAuthUrl = "oauth";
    }

    public static class DataTypes
    {
        public const string Bool = "bool";
        public const string InterpolationString = "interpolationString";
        public const string DateTime = "dateTime";
    }
}
