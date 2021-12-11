using System.Text.Json.Serialization;

namespace Zurich.Connector.Web.Models
{
    public class DataSourceRegistrationRequestViewModel
    {
        /// <summary>
        /// The domain for a given data source (Only required in certain situations)
        /// </summary>
        public string Domain { get; set; }

    }
}
