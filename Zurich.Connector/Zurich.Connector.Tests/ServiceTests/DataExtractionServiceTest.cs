using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.App;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Tests.Common;
using Zurich.TenantData;

namespace Zurich.Connector.Tests.ServiceTests
{
    [TestClass]
    public class DataExtractionServiceTest
    {
        private Mock<ICosmosService> _mockCosmosService;
        private Mock<IDataMappingFactory> _mockDataMappingFactory;
        private IMapper _mapper;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockDataMappingFactory = new Mock<IDataMappingFactory>();
            var mapConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ServiceMappingRegistrar());
            });
            _mapper = mapConfig.CreateMapper();
            _mockCosmosService = new Mock<ICosmosService>();
        }

        [TestMethod]
        public async Task CallExtractHeadersParams()
        {
            // ARRANGE
            var queryParameters = new Dictionary<string, string>() { { "Email", "fakeemail@test.com" }};
            ConnectorModel connectorModel = new ConnectorModel()
            {
                Request = new ConnectorRequestModel()
                {
                    EndpointPath = "/testapi/v1/users/{42.id}/action",
                    Parameters = new List<ConnectorRequestParameterModel>()
                    {
                            new ConnectorRequestParameterModel() { CdmName = "Email", Name = "$filter", DefaultValue = ""},
                    },
                },
                 
            };
            connectorModel.CDMMapping = new CDMMappingModel()
             {
                 Structured = new List<CDMElementModel>() {
                         {
                             new CDMElementModel(){ Name="Id", ResponseElement ="userId"}
                         },
                         {
                             new CDMElementModel(){ Name="Email", ResponseElement ="email"}
                         },
                         {
                             new CDMElementModel(){ Name="UserType", ResponseElement ="dataCenterId"}
                         }
                 }
             };
            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);

            DataExtractionService service = new DataExtractionService(_mockCosmosService.Object, _mapper, _mockDataMappingFactory.Object);

            // ACT
            _mockCosmosService.Setup(x => x.GetConnector("UserInfo", true)).Returns(Task.FromResult(connectorModel));
            Dictionary<string, string> headerParameters = service.ExtractParams(queryParameters, connectorDocument, InClauseConstants.Headers);

            // ASSERT
            Assert.AreEqual(queryParameters.Values.Count, 1);
        }

        [TestMethod]
        public async Task CallExtrPactathParams()
        {
            // ARRANGE
            var queryParameters = new Dictionary<string, string>() { { "CustomerId", "1" }, { "LibraryName", "library" }, { "FolderId", "folder123" } };

            ConnectorModel connectorModel = new ConnectorModel()
            {
                Request = new ConnectorRequestModel()
                {
                    EndpointPath = "work/api/v2/customers/api/v2/customers/{{customerId}}/libraries/{{libraryName}}/folders/{{folderId}}/documents",
                    Parameters = new List<ConnectorRequestParameterModel>()
                    {
                            new ConnectorRequestParameterModel() { CdmName = "CustomerId", Name = "CustomerId", InClause="Path", DefaultValue = ""},
                            new ConnectorRequestParameterModel() { CdmName = "LibraryName", Name = "LibraryName", InClause="Path", DefaultValue = ""},
                            new ConnectorRequestParameterModel() { CdmName = "FolderId", Name = "FolderId", InClause="Path", DefaultValue = ""}
                    },
                },

            };
            connectorModel.CDMMapping = new CDMMappingModel()
            {
                Structured = new List<CDMElementModel>() {
                         {
                             new CDMElementModel(){ Name="AuthorName", ResponseElement ="author_description"}
                         },
                         {
                             new CDMElementModel(){ Name="author", ResponseElement ="author"}
                         },
                         {
                             new CDMElementModel(){ Name="checksum", ResponseElement ="checksum"}
                         }
                 }
            };
            ConnectorDocument connectorDocument = _mapper.Map<ConnectorDocument>(connectorModel);

            DataExtractionService service = new DataExtractionService(_mockCosmosService.Object, _mapper, _mockDataMappingFactory.Object);

            // ACT
            _mockCosmosService.Setup(x => x.GetConnector("UserInfo", true)).Returns(Task.FromResult(connectorModel));
            Dictionary<string, string> pathParameters = service.ExtractParams(queryParameters, connectorDocument, InClauseConstants.Path);

            // ASSERT
            Assert.AreEqual(queryParameters.Values.Count, 3);
        }
    }
}


