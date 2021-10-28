namespace Zurich.Connector.Data.Model
{
    public class DataSourceInformation
    {
        /// <summary>
        /// Name of the datasource. Ex: "Office 365"
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// AppCode. Ex: "MSGraph"
        /// </summary>
        public string AppCode { get; set; }
        /// <summary>
        /// Domain. Ex: "graph.microsoft.com"
        /// </summary>
        public string Domain { get; set; }
    }
}
