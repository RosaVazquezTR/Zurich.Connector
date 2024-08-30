namespace Zurich.Connector.Data.Model
{
    /// <summary>
    /// Class that streamlines some the Tenant information for a datasource
    /// </summary>
    public class DataSourceTenantInformation
    {
        /// <summary>
        /// TenantID of the Application
        /// </summary>
        public string AppTenantId { get; set; }
        /// <summary>
        /// Flag to send Tenant in Body
        /// </summary>
        public bool SendTenantInBody { get; set; }
        /// <summary>
        /// LAPS TenantID
        /// </summary>
        public string TenantId { get; set; }
        /// <summary>
        /// Appcode
        /// </summary>
        public string ApplicationCode { get; set; }
    }
}
