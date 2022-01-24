using AutoMapper;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data.Repositories;

namespace Zurich.Connector.App.Services
{
    public interface IOAuthApiServices
    {
        /// <summary>
        /// Gets the tenant aware partner app token from the data store
        /// </summary>
        /// <param name="appCode">The partner app code</param>
        /// <returns>A <see cref="Token"/> containing the accessToken</returns>
        public Task<Token> GetToken(string appCode);
    }
    public class OAuthApiServices : IOAuthApiServices
    {
        private readonly IOAuthApiRepository _OAuthApiRespository;
        private readonly IMapper _mapper;

        public OAuthApiServices(IOAuthApiRepository OAuthApiRepository, IMapper mapper)
        {
            _OAuthApiRespository = OAuthApiRepository;
            _mapper = mapper;
        }
        public async Task<Token> GetToken(string appCode)
        {
            var apiResponse = await _OAuthApiRespository.GetToken(appCode);
            var mappedResponse = _mapper.Map<Token>(apiResponse);
            return mappedResponse;
        }
    }
}
