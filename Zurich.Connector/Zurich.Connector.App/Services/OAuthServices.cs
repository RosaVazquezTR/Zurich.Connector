using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Repositories;

namespace Zurich.Connector.App.Services
{
   
    public interface IOAuthServices
    {
        /// <summary>
        /// Get the Automatic registration status from OAuth Service
        /// </summary>
        /// <param name="appCode">using appCode parameter passing proper application code.</param>
        /// <returns>Return the Status of autoregistration</returns>
        Task<bool> AutomaticRegistration(string appCode);
    }
    public class OAuthServices :IOAuthServices
    {
        private readonly IOAuthRepository _OAuthRespository;

        public OAuthServices(IOAuthRepository OAuthRepository)
        {
            _OAuthRespository = OAuthRepository;
        }
        public async Task<bool> AutomaticRegistration(string appCode)
        {
            return await _OAuthRespository.AutomaticRegistration(appCode);
        }
    }
}
