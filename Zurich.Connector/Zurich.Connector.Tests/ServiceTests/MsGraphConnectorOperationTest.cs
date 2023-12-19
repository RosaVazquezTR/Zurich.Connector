using AutoMapper;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.App.Services;
using Zurich.Connector.App.Services.DataSources;
using Zurich.Connector.Data;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;

namespace Zurich.Connector.Tests.ServiceTests
{
    [TestClass]
    public class MsGraphConnectorOperationTest
    {
        private Mock<ILogger<PracticalLawConnectorOperation>> _mockLogger;
        private Mock<IDataMapping> _mockDataMapping;
        private Mock<IDataMappingFactory> _mockDataMappingFactory;
        private IConfiguration _mockConfiguration;
        private string _msAppCode = KnownDataSources.msGraph;
        private Mock<ICosmosService> _mockCosmosService;
        private Mock<IMapper> _mockMapper;

        #region Data Setup
        internal static IEnumerable<dynamic> HappyPath()
        {
            dynamic result;

            result = JObject.Parse(@"{
                ""TotalCount"": 2,
                ""Documents"": [
                    {
	                    ""Snippet"": ""<c0>the</c0> team site document <ddd/>"",
	                    ""Title"": ""teamSiteexcel.xlsx"",
	                    ""Type"": null,
	                    ""WebUrl"": ""https://rhdevtenant.sharepoint.com/sites/Teamsite/Shared Documents/teamSiteexcel.xlsx"",
	                    ""CreationDate"": ""2021-10-19T16:23:52Z"",
	                    ""AdditionalProperties"": {
		                    ""lastModifiedDateTime"": ""2021-10-19T16:24:32Z"",
		                    ""authorName"": ""Ryan Hunecke""
	                    }
                    },
                    {
	                    ""Snippet"": ""<c0>in</c0> a sharepoint library Doing lots of cool stuff <c0>in</c0> here <ddd/>"",
	                    ""Title"": ""SharepointLibrary doc.docx"",
	                    ""Type"": null,
	                    ""WebUrl"": ""https://rhdevtenant.sharepoint.com/sites/Testinglibrary/Shared Documents/SharepointLibrary doc.docx"",
	                    ""CreationDate"": ""2021-10-19T16:11:14Z"",
	                    ""AdditionalProperties"": {
		                    ""lastModifiedDateTime"": ""2021-10-19T16:12:44Z"",
		                    ""authorName"": ""Ryan Hunecke""
	                    }
                    }
                ]
            }");
            return result;
        }

        internal static IEnumerable<dynamic> NoTitle()
        {
            dynamic result;

            result = JObject.Parse(@"{
                ""TotalCount"": 2,
                ""Documents"": [
                    {
	                    ""Snippet"": ""<c0>the</c0> team site document <ddd/>"",
	                    ""Type"": null,
	                    ""WebUrl"": ""https://rhdevtenant.sharepoint.com/sites/Teamsite/Shared Documents/teamSiteexcel.xlsx"",
	                    ""CreationDate"": ""2021-10-19T16:23:52Z"",
	                    ""AdditionalProperties"": {
		                    ""lastModifiedDateTime"": ""2021-10-19T16:24:32Z"",
		                    ""authorName"": ""Ryan Hunecke""
	                    }
                    },
                    {
	                    ""Snippet"": ""<c0>in</c0> a sharepoint library Doing lots of cool stuff <c0>in</c0> here <ddd/>"",
	                    ""Title"": ""SharepointLibrary doc.docx"",
	                    ""Type"": null,
	                    ""WebUrl"": ""https://rhdevtenant.sharepoint.com/sites/Testinglibrary/Shared Documents/SharepointLibrary doc.docx"",
	                    ""CreationDate"": ""2021-10-19T16:11:14Z"",
	                    ""AdditionalProperties"": {
		                    ""lastModifiedDateTime"": ""2021-10-19T16:12:44Z"",
		                    ""authorName"": ""Ryan Hunecke""
	                    }
                    }
                ]
            }");
            return result;
        }

        internal static IEnumerable<dynamic> TitleNoExtension()
        {
            dynamic result;

            result = JObject.Parse(@"{
                ""TotalCount"": 1,
                ""Documents"": [
                    {
	                    ""Snippet"": ""<c0>the</c0> team site document <ddd/>"",
	                    ""Title"": ""Team site"",
	                    ""Type"": null,
	                    ""WebUrl"": ""https://rhdevtenant.sharepoint.com/sites/Teamsite/Shared Documents/teamSiteexcel.xlsx"",
	                    ""CreationDate"": ""2021-10-19T16:23:52Z"",
	                    ""AdditionalProperties"": {
		                    ""lastModifiedDateTime"": ""2021-10-19T16:24:32Z"",
		                    ""authorName"": ""Ryan Hunecke""
	                    }
                    }
                ]
            }");
            return result;
        }

        internal static JObject WebUrlVerification()
        {
            JObject result;

            result = JObject.Parse(@"{
                ""TotalCount"": 2,
                ""Documents"": [
                    {
	                    ""Snippet"": ""<c0>the</c0> team site document <ddd/>"",
	                    ""Title"": ""teamSiteexcel.pdf"",
	                    ""Type"": null,
	                    ""WebUrl"": ""https://rhdevtenant.sharepoint.com/sites/Teamsite/Shared Documents/teamSiteexcel.pdf"",
	                    ""CreationDate"": ""2021-10-19T16:23:52Z"",
	                    ""AdditionalProperties"": {
		                    ""lastModifiedDateTime"": ""2021-10-19T16:24:32Z"",
		                    ""authorName"": ""Ryan Hunecke""
	                    }
                    },
                    {
                        ""Snippet"": ""<b>Test</b> settings Notes Index current index None 2020-08-26T21:20:00.287Z 2020-08-26T21:22:06.<ddd/><b>test</b> 2020-08-26T23:14:31.195Z 2020-08-27T06:12:45.755Z 1 every 4 calls 65 1500000 3000<ddd/>"",
                        ""Title"": ""cosmostimings.xlsx"",
	                    ""Type"": null,
                        ""WebUrl"": ""https://rhdevtenant.sharepoint.com/sites/contentTypeHub/Shared Documents/cosmostimings.xlsx"",
                        ""CreationDate"": ""2021-10-19T16:26:58Z"",
                        ""AdditionalProperties"": {
                            ""lastModifiedDateTime"": ""2020-08-27T22:07:08Z"",
                            ""lastModifiedUser"": ""Ryan Hunecke"",
                            ""authorName"": ""Ryan Hunecke"",
                            ""listItemUniqueId"": ""C41B7430-07CC-4174-B1DA-97193AD2692E""
                        }
                    }
                ]
            }");
            return result;
        }
        #endregion Data Setup

        [TestInitialize]
        public void Init()
        {
            _mockLogger = new Mock<ILogger<PracticalLawConnectorOperation>>();
            _mockDataMapping = new Mock<IDataMapping>();
            _mockDataMappingFactory = new Mock<IDataMappingFactory>();
            _mockCosmosService = new Mock<ICosmosService>();
            _mockMapper = new Mock<IMapper>();
        }

        public MsGraphConnectorOperation GetService()
        {
            return new MsGraphConnectorOperation(_mockLogger.Object, _mockDataMappingFactory.Object, _mockConfiguration, _mockCosmosService.Object, _mockMapper.Object);
        }

        [TestMethod]
        public async Task SetItemLinkTest_Should_RemoveExtensionSetExtensionAndUpdateSnippet()
        {
            //Arrange
            var expectedType = "Excel";
            var expectedExtension = "xlsx";
            var expectedTitle = "teamSiteexcel";
            var expectedSnippet = "<b>the</b> team site document. ";

            var mockDocuments = HappyPath();
            var service = GetService();

            //Act
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Search, mockDocuments, _msAppCode, null) as JObject);

            //Assert
            result.Should().NotBeNull();
            var doc = result["Documents"][0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.Type).Should().BeTrue();
            doc.ContainsKey(StructuredCDMProperties.Title).Should().BeTrue();
            doc.ContainsKey(StructuredCDMProperties.Snippet).Should().BeTrue();
            doc.ContainsKey(StructuredCDMProperties.AdditionalProperties).Should().BeTrue();
            doc[StructuredCDMProperties.Type].Value<string>().Should().Be(expectedType);
            doc[StructuredCDMProperties.Title].Value<string>().Should().Be(expectedTitle);
            doc[StructuredCDMProperties.Snippet].Value<string>().Should().Be(expectedSnippet);
            doc[StructuredCDMProperties.AdditionalProperties][UnstructuredCDMProperties.Extension].Value<string>().Should().Be(expectedExtension);
        }

        [TestMethod]
        public async Task SetItemLinkTest_Should_UpdateSnippet()
        {
            //Arrange
            var expectedType = "Word";
            var expectedExtension = "docx";
            var expectedTitle = "SharepointLibrary doc";
            var expectedSnippet1 = "<b>the</b> team site document. ";
            var expectedSnippet2 = "<b>in</b> a sharepoint library Doing lots of cool stuff <b>in</b> here. ";

            var mockDocuments = NoTitle();
            var service = GetService();

            //Act
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Search, mockDocuments, _msAppCode, null) as JObject);

            //Assert
            result.Should().NotBeNull();
            var doc = result["Documents"][0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.Type).Should().BeTrue();
            doc.ContainsKey(StructuredCDMProperties.Title).Should().BeFalse();
            doc.ContainsKey(StructuredCDMProperties.Snippet).Should().BeTrue();
            doc[StructuredCDMProperties.Type].Value<string>().Should().BeNull();
            doc[StructuredCDMProperties.Snippet].Value<string>().Should().Be(expectedSnippet1);
            var doc2 = result["Documents"][1] as JObject;
            doc2.ContainsKey(StructuredCDMProperties.Type).Should().BeTrue();
            doc2.ContainsKey(StructuredCDMProperties.Title).Should().BeTrue();
            doc2.ContainsKey(StructuredCDMProperties.Snippet).Should().BeTrue();
            doc2.ContainsKey(StructuredCDMProperties.AdditionalProperties).Should().BeTrue();
            doc2[StructuredCDMProperties.Type].Value<string>().Should().Be(expectedType);
            doc2[StructuredCDMProperties.Title].Value<string>().Should().Be(expectedTitle);
            doc2[StructuredCDMProperties.Snippet].Value<string>().Should().Be(expectedSnippet2);
            doc2[StructuredCDMProperties.AdditionalProperties][UnstructuredCDMProperties.Extension].Value<string>().Should().Be(expectedExtension);
        }

        [TestMethod]
        public async Task SetItemLinkTest_Should_UpdateSnippetNoExtension()
        {
            //Arrange
            var expectedTitle = "Team site";
            var expectedSnippet = "<b>the</b> team site document. ";

            var mockDocuments = TitleNoExtension();
            var service = GetService();

            //Act
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Search, mockDocuments, _msAppCode, null) as JObject);

            //Assert
            result.Should().NotBeNull();
            var doc = result["Documents"][0] as JObject;
            doc.ContainsKey(StructuredCDMProperties.Type).Should().BeTrue();
            doc.ContainsKey(StructuredCDMProperties.Title).Should().BeTrue();
            doc.ContainsKey(StructuredCDMProperties.Snippet).Should().BeTrue();
            doc[StructuredCDMProperties.Type].Value<string>().Should().Be(string.Empty);
            doc[StructuredCDMProperties.Title].Value<string>().Should().Be(expectedTitle);
            doc[StructuredCDMProperties.Snippet].Value<string>().Should().Be(expectedSnippet);
        }

        //
        [TestMethod]
        public async Task SetWebUrlTest_Should_UpdateUrlOnlyifNotPdf()
        {
            //Arrange
            var expectedWebUrl = "https://rhdevtenant.sharepoint.com/sites/contentTypeHub/_layouts/15/Doc.aspx?sourcedoc=%7BC41B7430-07CC-4174-B1DA-97193AD2692E%7D&file=cosmostimings.xlsx&action=default&mobileredirect=true&DefaultItemOpen=1";

            var mockDocuments = WebUrlVerification();
            var service = GetService();

            //Act
            var result = (await service.SetItemLink(Data.Model.ConnectorEntityType.Search, mockDocuments, _msAppCode, null) as JObject);

            //Assert
            result.Should().NotBeNull();
            var pdfDoc = result["Documents"][0] as JObject;
            pdfDoc.ContainsKey(StructuredCDMProperties.WebUrl).Should().BeTrue();
            Assert.AreEqual(mockDocuments["Documents"][0][StructuredCDMProperties.WebUrl], pdfDoc[StructuredCDMProperties.WebUrl]);

            var otherDoc = result["Documents"][1] as JObject;
            otherDoc.ContainsKey(StructuredCDMProperties.WebUrl).Should().BeTrue();
            Assert.AreEqual(expectedWebUrl, otherDoc[StructuredCDMProperties.WebUrl]);
        }
    }
}
