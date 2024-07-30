using System.Threading.Tasks;
using Zurich.Connector.Data.Model;
using System.Collections.Specialized;
using System.IO;

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
        /// Handles the success response for outside party api downloading document content
        /// </summary>
        /// <param name="data"></param>
        /// <param name="transformToPdf"></param>
        /// <returns>A string containing the content of the document divided by page and in base 64</returns>
        Task<string> HandleSuccessResponse(Stream documentStream, bool transformToPdf = true);
    }
}
