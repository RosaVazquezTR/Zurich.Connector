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
    [TestClass]
    public class LexisNexisUKConnectorOperationTests
    {
        private Mock<ILogger<LexisNexisUKConnectorOperation>> _mockLogger;

        [TestInitialize]
        public void Init()
        {
            _mockLogger = new Mock<ILogger<LexisNexisUKConnectorOperation>>();

        }

        [TestMethod]
        public async Task SetItemLinkTest_Should_SetWebUrl()
        {
            //Arrange
            var mockDocuments = MockConnectorData.SetupLexisDocumentsModel();
            string documentPath = "/shared/document/cases-uk/urn:contentItem:4MFT-X600-TWW4-20TB-00000-00";
            var expectedUrl = $"https://plus.lexis.com/uk/document?pddocfullpath={documentPath}";
            //Act
            var service = new LexisNexisUKConnectorOperation(_mockLogger.Object);
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Search, mockDocuments, "LexisNexisUK", null) as JObject);
            
            //Assert
            result.Should().NotBeNull();
            var doc = result["Documents"][0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.WebUrl).Should().BeTrue();
            doc[StructuredCDMProperties.WebUrl].Value<string>().Should().Be(expectedUrl);
        }
    }
}
