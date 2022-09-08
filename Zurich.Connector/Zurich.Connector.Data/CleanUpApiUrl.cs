namespace Zurich.Connector.Data
{
    public class CleanUpApiUrl
    {
        public static string FormattingUrl(string hostName)
        {
            if (hostName.Contains("https://"))
            {
                hostName = hostName.Replace("https://", "");
            }
            if (hostName.EndsWith("/"))
            {
                hostName = hostName.Remove(hostName.Length - 1);
            }
            return hostName;
        }
    }
}
