using System.Text.Json.Serialization;

namespace Zurich.Connector.Web.Models
{
    public class DataSourceRegistrationResponseViewModel
    {
        /// <summary>
        /// Authorize url so that a user can log into a 3rd party
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string? AuthorizeUrl { get; set; }

        /// <summary>
        /// True if the dataSource was registered.
        /// </summary>
        public bool Registered { get; set; }
    }
}
