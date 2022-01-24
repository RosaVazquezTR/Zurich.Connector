using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zurich.Common.Repositories;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Repositories;

namespace Zurich.Connector.Data.DataMap
{
    /// <summary>
    /// Used mostly in tests
    /// </summary>
    public class DataMapping : AbstractDataMapping, IDataMapping
    {
        public DataMapping(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, IHttpBodyFactory httpBodyFactory, IHttpResponseFactory httpResponseFactory, ILogger<DataMapping> logger, IHttpContextAccessor contextAccessor, IOAuthApiRepository OAuthApirepository, ILegalHomeAccessCheck legalHomeAccessCheck, IConfiguration configuration)
        {
            this._repository = repository;
            this._dataMappingRepository = dataMappingRepository;
            this._oAuthService = oAuthService;
            this._logger = logger;
            this._httpResponseFactory = httpResponseFactory;
            this._httpBodyFactory = httpBodyFactory;
            this._contextAccessor = contextAccessor;
            this._oAuthApirepository = OAuthApirepository;
            this._legalHomeAccessCheck = legalHomeAccessCheck;
            this._configuration = configuration;
        }
    }
}
