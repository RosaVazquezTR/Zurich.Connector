using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Zurich.Common;
using Zurich.Connector.App.Services.DataSources;
using Zurich.Connector.Data;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Tests.Common;

namespace Zurich.Connector.Tests.ServiceTests
{
    [TestClass]
    public class PracticalLawConnectorOperationsTest
    {
        private Mock<ILogger<PracticalLawConnectorOperation>> _mockLogger;
        private Mock<IDataMapping> _mockDataMapping;
        private Mock<IDataMappingFactory> _mockDataMappingFactory;
        private IConfiguration _mockConfiguration;

        [TestInitialize]
        public void Init()
        {
            _mockLogger = new Mock<ILogger<PracticalLawConnectorOperation>>();
            _mockDataMapping = new Mock<IDataMapping>();
            _mockDataMappingFactory = new Mock<IDataMappingFactory>();
        }

        [TestMethod]
        public async Task SetItemLinkTest_Should_SetWebUrl()
        {
            //Arrange
            var mockDocuments = MockConnectorData.SetupSearchDocumentsModel();
            var hostName = "practicallawconnect.com";
            var appCode = "PracticalLawConnect";
            var PlcReference = "4-000-4131";
            var expectedUrl = $"https://{hostName}/{PlcReference}";
            //Act
            var token = new JObject();
            _mockConfiguration = Utility.CreateConfiguration(AppConfigKeys.PracticalLawConnectSearchHost, "practicallawconnect.com");

            _mockDataMapping.Setup(x => x.GetAndMapResults<JToken>(It.IsAny<ConnectorDocument>(), string.Empty, null, null)).Returns(Task.FromResult((JToken)token));
            _mockDataMappingFactory.Setup(x => x.GetImplementation(Data.Model.AuthType.OAuth2.ToString())).Returns(_mockDataMapping.Object);
            var service = new PracticalLawConnectorOperation(_mockLogger.Object, _mockDataMappingFactory.Object, _mockConfiguration);
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Search, mockDocuments, appCode, hostName) as JObject);
            //Assert
            result.Should().NotBeNull();
            var doc = result["Documents"][0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.WebUrl).Should().BeTrue();
            doc[StructuredCDMProperties.WebUrl].Value<string>().Should().Be(expectedUrl);
        }

        [TestMethod]
        public async Task SetItemLinkTestShouldLogErrorAndReturnOriginalEntitiesIfHostnameIsInvalid()
        {
            //Arrange
            var mockDocuments = MockConnectorData.SetupSearchDocumentsModel();
            var appCode = "PracticalLawConnect";

            //Act
            var token = new JObject();
            _mockConfiguration = Utility.CreateConfiguration("FakeKey", "practicallawconnect.com");

            _mockDataMapping.Setup(x => x.GetAndMapResults<JToken>(It.IsAny<ConnectorDocument>(), string.Empty, null, null)).Returns(Task.FromResult((JToken)token));
            _mockDataMappingFactory.Setup(x => x.GetImplementation(Data.Model.AuthType.OAuth2.ToString())).Returns(_mockDataMapping.Object);
            var service = new PracticalLawConnectorOperation(_mockLogger.Object, _mockDataMappingFactory.Object, _mockConfiguration);
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Search, mockDocuments, appCode, null) as JObject);
            //Assert
            result.Should().NotBeNull();
            var doc = result["Documents"][0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.WebUrl).Should().BeTrue();
            doc[StructuredCDMProperties.WebUrl].Value<string>().Should().Be("");

        }
    }
}
