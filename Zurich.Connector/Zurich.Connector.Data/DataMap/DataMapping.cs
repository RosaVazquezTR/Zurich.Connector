using Microsoft.Extensions.Logging;
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
        public DataMapping(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, IHttpBodyFactory httpBodyFactory, IHttpResponseFactory httpResponseFactory, ILogger<DataMapping> logger)
        {
            this._repository = repository;
            this._dataMappingRepository = dataMappingRepository;
            this._oAuthService = oAuthService;
            this._logger = logger;
            this._httpResponseFactory = httpResponseFactory;
            this._httpBodyFactory = httpBodyFactory;
        }
    }
}
