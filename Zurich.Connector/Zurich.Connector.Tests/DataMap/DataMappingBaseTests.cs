using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.ProductData.Models;
using AutoMapper;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Common.Repositories.Cosmos;
using Zurich.Connector.Data.Services;

namespace Zurich.Connector.Tests
{
	public class TestDataMapping: DataMappingBase
    {
		public TestDataMapping(IRepository repository, IDataMappingRepository dataMappingRepository, IOAuthService oAuthService, ILogger<DataMappingOAuth> logger, ConnectorCosmosContext connectorCosmosContext, IMapper mapper)
		{
			this._repository = repository;
			this._dataMappingRepository = dataMappingRepository;
			this._oAuthService = oAuthService;
			this._logger = logger;
			this._cosmosContext = connectorCosmosContext;
			this._mapper = mapper;
		}
	}

    [TestClass]
    public class DataMappingBaseTests
    {
        private Mock<IRepository> _mockRepository;
        private Mock<IDataMappingRepository> _mockDataMappingRepository;
        private Mock<IOAuthService> _mockOAuthService;
        private Mock<ILogger<DataMappingOAuth>> _mockLoggerOAuth;
        private Mock<ILogger<DataMappingTransfer>> _mockLoggerTransfer;
        private Mock<ICosmosClientStore> _mockCosmosDocumentReader;
        private Mock<IMapper> _mockMapper;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockRepository = new Mock<IRepository>();
            _mockDataMappingRepository = new Mock<IDataMappingRepository>();
            _mockOAuthService = new Mock<IOAuthService>();
            _mockLoggerOAuth = new Mock<ILogger<DataMappingOAuth>>();
            _mockLoggerTransfer = new Mock<ILogger<DataMappingTransfer>>();

            // feels like this won't change
            AppToken token = new AppToken() { access_token = "fakeToken" };
            _mockOAuthService.Setup(x => x.GetToken(It.IsAny<string>(), It.IsAny<OAuthApplicationType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<ProductType>())).Returns(Task.FromResult(token));
            _mockCosmosDocumentReader = new Mock<ICosmosClientStore>();
            _mockMapper = new Mock<IMapper>();
        }

        [TestMethod]
        public void TestMapping()
        {
            // ARRANGE
            TestDataMapping dataMapping = new TestDataMapping(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerOAuth.Object, _mockCosmosDocumentReader.Object, _mockMapper.Object);

            ConnectorDocument document = new ConnectorDocument()
            {
                request = new ConnectorRequest()
                {
                    parameters = new List<ConnectorRequestParameter>()
                    {
                        new ConnectorRequestParameter() {
                            cdmname = "Query",
                            name = "requests.[].query.queryString",
                            inClause = "body"
                        },
                        new ConnectorRequestParameter() {
                            cdmname = "EntityType",
                            name = "requests.[].entityTypes.[]",
                            inClause = "body"
                        }
                    }
                }
            };

            NameValueCollection collection = new NameValueCollection();
            collection["requests.[].entityTypes.[]"] = "driveItems";
            collection["requests.[].query.queryString"] = "docs";

            // ACT
            var postBody = dataMapping.CreatePostBody(document, collection);

            // ASSERT
            Assert.AreEqual("{\"requests\":[{\"query\":{\"queryString\":\"docs\"},\"entityTypes\":[\"driveItems\"]}]}", postBody);
        }

        [TestMethod]
        public void TestMappingMultiParam()
        {
            // ARRANGE
            TestDataMapping dataMapping = new TestDataMapping(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerOAuth.Object, _mockCosmosDocumentReader.Object, _mockMapper.Object);

            ConnectorDocument document = new ConnectorDocument()
            {
                request = new ConnectorRequest()
                {
                    parameters = new List<ConnectorRequestParameter>()
                    {
                        new ConnectorRequestParameter() {
                            cdmname = "Query",
                            name = "requests.[].query.queryString",
                            inClause = "body"
                        },
                        new ConnectorRequestParameter() {
                            cdmname = "EntityType",
                            name = "requests.[].entityTypes.[]",
                            inClause = "body"
                        },
                        new ConnectorRequestParameter() {
                            cdmname = "Fields",
                            name = "requests.[].fields.[]",
                            inClause = "body"
                        }
                    }
                }
            };

            NameValueCollection collection = new NameValueCollection();
            collection["requests.[].entityTypes.[]"] = "driveItems";
            collection["requests.[].query.queryString"] = "docs";
            collection["requests.[].fields.[]"] = "Name,CreatedDateTime,LastModifiedDateTime,LastModifiedBy,WebUrl,ParentReference";

            // ACT
            var postBody = dataMapping.CreatePostBody(document, collection);

            // ASSERT
            Assert.AreEqual("{\"requests\":[{\"query\":{\"queryString\":\"docs\"},\"entityTypes\":[\"driveItems\"],\"fields\":[\"Name\",\"CreatedDateTime\",\"LastModifiedDateTime\",\"LastModifiedBy\",\"WebUrl\",\"ParentReference\"]}]}", postBody);
        }

        [TestMethod]
        public void TestMappingNullValues()
        {
            // ARRANGE
            TestDataMapping dataMapping = new TestDataMapping(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerOAuth.Object, _mockCosmosDocumentReader.Object, _mockMapper.Object);

            ConnectorDocument document = new ConnectorDocument()
            {
                request = new ConnectorRequest()
                {
                    parameters = new List<ConnectorRequestParameter>()
                    {
                        new ConnectorRequestParameter() {
                            cdmname = "Query",
                            name = "requests.[].query.queryString",
                            inClause = "body"
                        },
                        new ConnectorRequestParameter() {
                            cdmname = "EntityType",
                            name = "requests.[].entityTypes.[]",
                            inClause = "body"
                        },
                        new ConnectorRequestParameter() {
                            cdmname = "Fields",
                            name = "requests.[].fields.[]",
                            inClause = "body"
                        }
                    }
                }
            };

            NameValueCollection collection = new NameValueCollection();
            collection["requests.[].entityTypes.[]"] = "driveItems";
            collection["requests.[].query.queryString"] = "docs";
            collection["requests.[].fields.[]"] = null;

            // ACT
            var postBody = dataMapping.CreatePostBody(document, collection);

            // ASSERT
            Assert.AreEqual("{\"requests\":[{\"query\":{\"queryString\":\"docs\"},\"entityTypes\":[\"driveItems\"],\"fields\":[]}]}", postBody);
        }

    }
}
