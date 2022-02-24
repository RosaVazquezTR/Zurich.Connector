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

            _mockDataMapping.Setup(x => x.GetAndMapResults<JToken>(It.IsAny<ConnectorDocument>(), string.Empty, null, null, null)).Returns(Task.FromResult((JToken)token));
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

            _mockDataMapping.Setup(x => x.GetAndMapResults<JToken>(It.IsAny<ConnectorDocument>(), string.Empty, null, null, null)).Returns(Task.FromResult((JToken)token));
            _mockDataMappingFactory.Setup(x => x.GetImplementation(Data.Model.AuthType.OAuth2.ToString())).Returns(_mockDataMapping.Object);
            var service = new PracticalLawConnectorOperation(_mockLogger.Object, _mockDataMappingFactory.Object, _mockConfiguration);
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Search, mockDocuments, appCode, null) as JObject);
            //Assert
            result.Should().NotBeNull();
            var doc = result["Documents"][0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.WebUrl).Should().BeTrue();
            doc[StructuredCDMProperties.WebUrl].Value<string>().Should().Be("");

        }

        [TestMethod]
        public async Task SetItemLinkTest_Should_SetplcReference_with_AdditionalProperties()
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

            _mockDataMapping.Setup(x => x.GetAndMapResults<JToken>(It.IsAny<ConnectorDocument>(), string.Empty, null, null, null)).Returns(Task.FromResult((JToken)token));
            _mockDataMappingFactory.Setup(x => x.GetImplementation(Data.Model.AuthType.OAuth2.ToString())).Returns(_mockDataMapping.Object);
            var service = new PracticalLawConnectorOperation(_mockLogger.Object, _mockDataMappingFactory.Object, _mockConfiguration);
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Search, mockDocuments, appCode, hostName) as JObject);
            //Assert
            result.Should().NotBeNull();
            var doc = result["Documents"][0] as JObject;
            doc[StructuredCDMProperties.WebUrl].Value<string>().Should().Be(expectedUrl);
            doc[StructuredCDMProperties.AdditionalProperties]["plcReference"].Value<string>().Should().Be(PlcReference);
        }

        [TestMethod]
        public async Task Search_Document_Type_Should_select_FirstValue_incase_Type_values_passing_as_Array()
        {
            //Arrange
            var mockDocuments = MockConnectorData.SetupSearchDocumentsModel_with_Document_Type_As_Array();
            var appCode = "PracticalLawConnect";

            //Act
            var token = new JObject();
            _mockConfiguration = Utility.CreateConfiguration("FakeKey", "practicallawconnect.com");

            _mockDataMapping.Setup(x => x.GetAndMapResults<JToken>(It.IsAny<ConnectorDocument>(), string.Empty, null, null, null)).Returns(Task.FromResult((JToken)token));
            _mockDataMappingFactory.Setup(x => x.GetImplementation(Data.Model.AuthType.OAuth2.ToString())).Returns(_mockDataMapping.Object);
            var service = new PracticalLawConnectorOperation(_mockLogger.Object, _mockDataMappingFactory.Object, _mockConfiguration);
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Search, mockDocuments, appCode, null) as JObject);
            //Assert
            result.Should().NotBeNull();
            var doc = result["Documents"][0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.WebUrl).Should().BeTrue();
            doc[StructuredCDMProperties.WebUrl].Value<string>().Should().Be("");
            doc[StructuredCDMProperties.Type].Type.ToString().Should().Be("String");
            doc[StructuredCDMProperties.Type].Value<string>().Should().Be("Practice notes");


        }
    }
}
