namespace Zurich.Connector.Data.Model
{
    public class OAuthAPITokenResponse
    {
        /// <summary>
        /// The access token type ex) bearer
        /// </summary>
        public string TokenType { get; set; }

        /// <summary>
        /// The token to be used
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Information in epoch time when the token expires.
        /// </summary>
        public long ExpiresOn { get; set; }
    }
}
