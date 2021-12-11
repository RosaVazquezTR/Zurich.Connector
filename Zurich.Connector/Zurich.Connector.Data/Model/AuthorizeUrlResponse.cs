namespace Zurich.Connector.Data.Model
{
    public class AuthorizeUrlResponse
    {
        /// <summary>
        /// the authorize URL for the OAuth process
        /// </summary>
        public string AuthorizeUrl { get; set; }
        /// <summary>
        /// the admin consent URL
        /// </summary>
        public string AdminConsentUrl { get; set; }
    }
}
