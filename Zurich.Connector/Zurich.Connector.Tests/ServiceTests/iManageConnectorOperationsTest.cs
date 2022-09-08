using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Zurich.Connector.App;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Services;
using Zurich.Connector.App.Services.DataSources;
using Zurich.Connector.Data;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Tests.Common;

namespace Zurich.Connector.Tests.ServiceTests
{
    //TODO: Create an app tests project and move this there

    [TestClass]
    public class IManageConnectorOperationsTest
    {
        private Mock<ILogger<IManageConnectorOperations>> _mockLogger;
        private Mock<IDataMapping> _mockDataMapping;
        private Mock<IDataMappingFactory> _mockDataMappingFactory;
        private Mock<ICosmosService> _mockCosmosService;
        private IMapper _mapper;

        [TestInitialize]
        public void Init()
        {
            _mockLogger = new Mock<ILogger<IManageConnectorOperations>>();
            _mockDataMappingFactory = new Mock<IDataMappingFactory>();
            _mockDataMapping = new Mock<IDataMapping>();
            _mockCosmosService = new Mock<ICosmosService>();

            var mapConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ServiceMappingRegistrar());
            });
            _mapper = mapConfig.CreateMapper();
        }

        [TestMethod]
        public async Task SetItemLinkTest_Should_SetWebUrl()
        {
            //Arrange
            var mockDocuments = MockConnectorData.SetupDocumentsModel();
            var hostName = "my.cookieapp.com";
            var appCode = "";
            var expectedUrl = $"https://{hostName}/work/link/d/1";
            var customerId = "1";
            //Act
            var token = new JObject();
            token["customer_id"] = customerId;
            _mockDataMapping.Setup(x => x.GetAndMapResults<JToken>(It.IsAny<ConnectorDocument>(), string.Empty, null, null, null, null)).Returns(Task.FromResult((JToken)token));
            _mockDataMappingFactory.Setup(x => x.GetImplementation(Data.Model.AuthType.OAuth2.ToString())).Returns(_mockDataMapping.Object);
            _mockCosmosService.Setup(x => x.GetConnector("1", true)).Returns(Task.FromResult(new ConnectorModel()));
            var service = new IManageConnectorOperations(_mockLogger.Object, _mockDataMappingFactory.Object, _mapper, _mockCosmosService.Object);
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Document, mockDocuments, appCode, hostName) as JObject);
            //Assert
            result.Should().NotBeNull();
            var doc = result["Items"][0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.WebUrl).Should().BeTrue();
            doc[StructuredCDMProperties.WebUrl].Value<string>().Should().Be(expectedUrl);
        }

        [TestMethod]
        public async Task SetItemLinkTest_Should_Log_Error_And_Return_Original_Entities_If_Hostname_Is_Invalid()
        {
            //Arrange
            var mockDocuments = MockConnectorData.SetupDocumentsModel();
            var customerId = "2";
            var appCode = "";
            //Act
            var token = new JObject();
            token["customer_id"] = customerId;
            _mockDataMapping.Setup(x => x.GetAndMapResults<JToken>(It.IsAny<ConnectorDocument>(), string.Empty, null, null, null, null)).Returns(Task.FromResult((JToken)token));
            _mockDataMappingFactory.Setup(x => x.GetImplementation(Data.Model.AuthType.OAuth2.ToString())).Returns(_mockDataMapping.Object);
            _mockCosmosService.Setup(x => x.GetConnector("1", true)).Returns(Task.FromResult(new ConnectorModel()));
            var service = new IManageConnectorOperations(_mockLogger.Object, _mockDataMappingFactory.Object, _mapper, _mockCosmosService.Object);
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Document, mockDocuments, appCode, null) as JObject);
            //Assert
            _mockLogger.Verify(ml => ml.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, _) => v.ToString().StartsWith("Unable to parse")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));
            result.Should().NotBeNull();
            var doc = result["Items"][0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.WebUrl).Should().BeFalse();
        }

        [TestMethod]
        public async Task SetItemLinkTest_Should_SetDownloadUrl()
        {
            //Arrange
            var mockDocuments = MockConnectorData.SetupDocumentsModel();
            var hostName = "my.cookieapp.com";
            var appCode = "";
            var customerId = "3";
            var libraryId = "TestLibrary";
            var docId = "1";
            var fileName = "Secretcookierecipe1";
            var expectedUrl = $"https://{hostName}/work/web/api/v2/customers/{customerId}/libraries/{libraryId}/documents/{docId}/download";
            //Act
            var token = new JObject();
            token["customer_id"] = customerId;
            _mockDataMapping.Setup(x => x.GetAndMapResults<JToken>(It.IsAny<ConnectorDocument>(), string.Empty, null, null, null, null)).Returns(Task.FromResult((JToken)token));
            _mockDataMappingFactory.Setup(x => x.GetImplementation(Data.Model.AuthType.OAuth2.ToString())).Returns(_mockDataMapping.Object);
            _mockCosmosService.Setup(x => x.GetConnector("1", true)).Returns(Task.FromResult(new ConnectorModel()));
            var service = new IManageConnectorOperations(_mockLogger.Object, _mockDataMappingFactory.Object, _mapper, _mockCosmosService.Object);
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Document, mockDocuments, appCode, hostName) as JObject);
            //Assert
            result.Should().NotBeNull();
            var doc = result["Items"][0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.DownloadUrl).Should().BeTrue();
            doc[StructuredCDMProperties.DownloadUrl].Value<string>().Should().Be(expectedUrl);
        }

        [TestMethod]
        public async Task SetItemLink_Should_SetCount()
        {
            //Arrange
            var mockSearchResult = MockConnectorData.SetupSearchDocumentsModel();
            short expectedCount = 2;
            //Act
            _mockDataMappingFactory.Setup(x => x.GetImplementation(Data.Model.AuthType.OAuth2.ToString())).Returns(_mockDataMapping.Object);
            _mockCosmosService.Setup(x => x.GetConnector("1", true)).Returns(Task.FromResult(new ConnectorModel()));
            var service = new IManageConnectorOperations(_mockLogger.Object, _mockDataMappingFactory.Object, _mapper, _mockCosmosService.Object);
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Search, mockSearchResult, null, null) as JObject);
            //Assert
            result.Should().NotBeNull();
            result[StructuredCDMProperties.ItemsCount].Value<short>().Should().Be(expectedCount);
        }


        [TestMethod]
        public async Task SetItemLinkTest_Search_Should_SetWebUrlAndType()
        {
            //Arrange
            var mockDocuments = MockConnectorData.SetupIManageSearchDocumentsModel();
            var hostName = "my.cookieapp.com";
            var appCode = "";
            var expectedUrl = $"https://{hostName}/work/link/d/ContractExpress!2218.1";
            var customerId = "1";
            //Act
            var token = new JObject();
            token["customer_id"] = customerId;
            _mockDataMapping.Setup(x => x.GetAndMapResults<JToken>(It.IsAny<ConnectorDocument>(), string.Empty, null, null, null, null)).Returns(Task.FromResult((JToken)token));
            _mockDataMappingFactory.Setup(x => x.GetImplementation(Data.Model.AuthType.OAuth2.ToString())).Returns(_mockDataMapping.Object);
            _mockCosmosService.Setup(x => x.GetConnector("1", true)).Returns(Task.FromResult(new ConnectorModel()));
            var service = new IManageConnectorOperations(_mockLogger.Object, _mockDataMappingFactory.Object, _mapper, _mockCosmosService.Object);
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Search, mockDocuments, appCode, hostName) as JObject);
            //Assert
            result.Should().NotBeNull();
            var doc = result["Documents"][0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.WebUrl).Should().BeTrue();
            doc[StructuredCDMProperties.WebUrl].Value<string>().Should().Be(expectedUrl);
            doc[StructuredCDMProperties.Type].Value<string>().Should().Be(ConnectorOperationsUtility.MapExtensionToDocumentType("TXT"));
        }


        [TestMethod]
        public async Task SetItemLinkTest_Search_Should_NoAdditionalProperties()
        {
            //Arrange
            var mockDocuments = MockConnectorData.SetupSearchDocumentsModel_NoAdditionalProperties();
            var hostName = "my.cookieapp.com";
            var appCode = "";
            var expectedUrl = $"https://{hostName}/work/link/d/ContractExpress!2218.1";
            var customerId = "1";
            //Act
            var token = new JObject();
            token["customer_id"] = customerId;
            _mockDataMapping.Setup(x => x.GetAndMapResults<JToken>(It.IsAny<ConnectorDocument>(), string.Empty, null, null, null, null)).Returns(Task.FromResult((JToken)token));
            _mockDataMappingFactory.Setup(x => x.GetImplementation(Data.Model.AuthType.OAuth2.ToString())).Returns(_mockDataMapping.Object);
            _mockCosmosService.Setup(x => x.GetConnector("1", true)).Returns(Task.FromResult(new ConnectorModel()));
            var service = new IManageConnectorOperations(_mockLogger.Object, _mockDataMappingFactory.Object, _mapper, _mockCosmosService.Object);
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Search, mockDocuments, appCode, hostName) as JObject);
            //Assert
            result.Should().NotBeNull();
            var doc = result["Documents"][0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.WebUrl).Should().BeTrue();
            doc[StructuredCDMProperties.WebUrl].Value<string>().Should().Be(string.Empty);
            doc.ContainsKey(StructuredCDMProperties.Type).Should().BeFalse();
        }
    }
}
