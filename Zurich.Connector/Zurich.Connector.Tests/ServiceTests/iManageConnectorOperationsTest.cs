using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Zurich.Connector.App.Services.DataSources;
using FluentAssertions;
using Newtonsoft.Json.Linq;
using Zurich.Connector.Tests.Common;
using Zurich.Connector.Data;
using System;
using System.Threading.Tasks;
using Zurich.Connector.Data.DataMap;
using Zurich.Common.Models.Connectors;
using Zurich.Connector.Data.Services;

namespace Zurich.Connector.Tests.ServiceTests
{
    //TODO: Create an app tests project and move this there

    [TestClass]
    public class IManageConnectorOperationsTest
    {
        private Mock<ILogger<IManageConnectorOperations>> _mockLogger;
        private Mock<IDataMappingFactory> _mockDataMappingFactory;

        [TestInitialize]
        public void Init()
        {
            _mockLogger = new Mock<ILogger<IManageConnectorOperations>>();
            _mockDataMappingFactory = new Mock<IDataMappingFactory>();
        }

        [TestMethod]
        public async Task SetItemLinkTest_Should_SetWebUrl()
        {
            //Arrange
            var mockDocuments = MockConnectorData.SetupDocumentsModel();
            var hostName = "my.cookieapp.com";
            var expectedUrl = $"https://{hostName}/work/link/d/1";
            //Act
            var service = new IManageConnectorOperations(_mockLogger.Object, _mockDataMappingFactory.Object);
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Document, mockDocuments, hostName) as JObject);
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
            //Act
            var service = new IManageConnectorOperations(_mockLogger.Object, _mockDataMappingFactory.Object);
            var result =  (await service.SetItemLink(Data.Model.ConnectorEntityType.Document, mockDocuments, null) as JObject);
            //Assert
            _mockLogger.Verify(ml => ml.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, _) => v.ToString().StartsWith("Unable to parse")), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));
            result.Should().NotBeNull();
            var doc = result["Items"][0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.WebUrl).Should().BeFalse();
        }
    }
}
