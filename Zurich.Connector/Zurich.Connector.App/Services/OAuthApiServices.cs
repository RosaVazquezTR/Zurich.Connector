using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;

namespace Zurich.Connector.App.Services
{
    public interface IOAuthApiServices
    {
        /// <summary>
        /// Gets the tenant aware partner app token from the data store
        /// </summary>
        /// <param name="appCode">The partner app code</param>
        /// <returns>A <see cref="Token"/> containing the access_token and refresh_token(if available)</returns>
        public Task<AppToken> GetToken(string appCode);
    }
    public class OAuthApiServices : IOAuthApiServices
    {
        private readonly IOAuthApiRepository _OAuthApiRespository;

        public OAuthApiServices(IOAuthApiRepository OAuthApiRepository)
        {
            _OAuthApiRespository = OAuthApiRepository;
        }
        public async Task<AppToken> GetToken(string appCode)
        {
            return await _OAuthApiRespository.GetToken(appCode);
        }
    }
}
