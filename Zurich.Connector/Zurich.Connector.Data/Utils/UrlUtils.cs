using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Utils
{
    /// <summary>
    /// Class to modify and format urls 
    /// </summary>
    public class UrlUtils
    {
        /// <summary>
        /// This function receives the api hostname and formats it in a way we can use it in our request
        /// </summary>
        /// <param name="hostName">The api hostname url</param>
        /// <returns>The hostname url formatted properly</returns>
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
