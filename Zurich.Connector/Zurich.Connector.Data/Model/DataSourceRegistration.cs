namespace Zurich.Connector.Data.Model
{
    public class DataSourceRegistration
    {
        /// <summary>
        /// Returns true if the registration was successful
        /// </summary>
        public bool Registered { get; set; }

        /// <summary>
        /// Authorize url if the data source was a manual registration
        /// </summary>
        public string? AuthorizeUrl { get; set; }
    }
}
