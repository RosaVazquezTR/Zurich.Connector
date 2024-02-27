using System.Threading.Tasks;
using Zurich.Connector.Data.Model;
using System.Collections.Specialized;

namespace Zurich.Connector.Data.Repositories
{
    /// <summary>
    /// Very generic Repository that should be able to make all API calls
    /// </summary>
    public interface IRepository
	{
        /// <summary>
        /// Makes a request to the outside party api
        /// </summary>
        /// <param name="apiInformation">Information about the api we are about to call</param>
        /// <param name="parameters">Query params that should be called in the API</param>
        /// <param name="body">body to send to the outside party</param>
        /// <returns></returns>
        Task<string> MakeRequest(ApiInformation apiInformation, NameValueCollection parameters, string body);

        /// <summary>
        /// Makes a request to the outside party api but only for downloading document content
        /// </summary>
        /// <param name="apiInformation"></param>
        /// <param name="transformToPDF"></param>
        /// <returns>A string containing the content of the document divided by page and in base 64</returns>
        Task<string> DocumentDownloadMakeRequest(ApiInformation apiInformation, bool transformToPDF=true);
	}
}
