namespace Zurich.Connector.App.Model
{
    /// <summary>
    /// Document download request model
    /// </summary>
    public class DocumentDownloadRequestModel
    {
        /// <summary>
        /// Gets or sets the connector id
        /// </summary>
        public string ConnectorId { get; set; }

        /// <summary>
        /// Gets or sets the document id
        /// </summary>
        public string DocId { get; set; }
    }
}