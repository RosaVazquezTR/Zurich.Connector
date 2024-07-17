using Moq;
using System.Threading.Tasks;
using Zurich.Connector.App.Services;
using Zurich.Common.Models.OAuth;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Repositories;
using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zurich.Common.Models.Connectors;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data.Model;
using System.Collections.Generic;
using Zurich.Connector.Data.DataMap;
using System.Collections.Specialized;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using System.IO;
using System;
using System.Text;

namespace Zurich.Connector.Tests.Services
{
    [TestClass]
    public class DocumentDownloadServiceTests
    {
        private Mock<IOAuthServices> _mockOAuthServices;
        private Mock<IDataMappingFactory> _mockDataMappingFactory;
        private Mock<IDataMappingService> _mockMappingService;
        private Mock<IRedisRepository> _mockRedisRepository;
        private Mock<IRepository> _mockRepository;
        private Mock<IDataExtractionService> _mockDataExtractionService;
        private Mock<IMapper> _mapperMock;
        private OAuthOptions _oAuthOptions;

        private DocumentDownloadService _service;

        [TestInitialize]
        public void Setup()
        {
            _mockOAuthServices = new Mock<IOAuthServices>();
            _mockDataMappingFactory = new Mock<IDataMappingFactory>();
            _mockMappingService = new Mock<IDataMappingService>();
            _mockRedisRepository = new Mock<IRedisRepository>();
            _mockRepository = new Mock<IRepository>();
            _mockDataExtractionService = new Mock<IDataExtractionService>();
            _mapperMock = new Mock<IMapper>();
            _oAuthOptions = new OAuthOptions();

            _service = new DocumentDownloadService(
                _mockOAuthServices.Object,
                _mockDataMappingFactory.Object,
                _mockMappingService.Object,
                _mockRedisRepository.Object,
                _oAuthOptions,
                _mockRepository.Object,
                _mockDataExtractionService.Object,
                _mapperMock.Object);
        }

        // TODO: Add some helper methods to streamline this a bit.
        [TestMethod]
        public async Task GetDocumentContentAsync_ReturnsDocumentContent()
        {
            // Arrange
            var connectorModel = new App.Model.ConnectorModel
            {
                Id = "CookieConnector",
                Info = new ConnectorInfoModel { DownloadConnector = "" },
                DataSource = new App.Model.DataSourceModel { Id = "NewDatasource" }
            };
            var downloadConnectorModel = new App.Model.ConnectorModel
            {
                Id = "CookieConnector",
                DataSource = new App.Model.DataSourceModel { Id = "NewDatasource" }
            };
            var userRegistrations = new DataSourceInformation
            {
                AppCode = connectorModel.Id
            };
            var dataMapping = new Mock<IDataMapping>();
            var expectedContent = "documentContent";
            var base64String = Convert.ToBase64String(Encoding.UTF8.GetBytes(expectedContent));

            _mockMappingService
                .SetupSequence(x => x.RetrieveProductInformationMap(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .ReturnsAsync(connectorModel)
                .ReturnsAsync(downloadConnectorModel);
            _mockOAuthServices
                .Setup(x => x.GetUserRegistrations())
                .ReturnsAsync([userRegistrations]);
            _mockDataMappingFactory
                .Setup(x => x.GetImplementation(It.IsAny<string>()))
                .Returns(dataMapping.Object);
            _mockDataExtractionService
                .Setup(x => x.ExtractParams(It.IsAny<Dictionary<string, string>>(), It.IsAny<ConnectorDocument>(), It.IsAny<string>()))
                .Returns([]);
            dataMapping
                .Setup(x => x.GetAndMapResults<dynamic>(It.IsAny<ConnectorDocument>(), It.IsAny<string>(), It.IsAny<NameValueCollection>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<string>()))
                .ReturnsAsync(base64String);
            _mockRepository
                .Setup(x => x.HandleSuccessResponse(It.IsAny<Stream>(), It.IsAny<bool>()))
                .ReturnsAsync(expectedContent);
            _mockRedisRepository
                .Setup(x => x.GetAsStringAsync(It.IsAny<string>()))
                .ReturnsAsync("");

            // Act
            string result = await _service.GetDocumentContentAsStringAsync(new DocumentDownloadRequestModel { ConnectorId = "ChocolateCookie", DocId = "docId" });

            // Assert
            Assert.AreEqual(expectedContent, result);
        }

        [TestMethod]
        public async Task GetDocumentContentAsync_ReturnsDocumentContent_InCache()
        {
            // Arrange
            _mockRedisRepository
                .Setup(x => x.GetAsStringAsync(It.IsAny<string>()))
                .ReturnsAsync("documentContent");

            // Act
            string result = await _service.GetDocumentContentAsStringAsync(new DocumentDownloadRequestModel { ConnectorId = "ChocolateCookie", DocId = "docId" });

            // Assert
            Assert.AreEqual("documentContent", result);
        }
    }
}