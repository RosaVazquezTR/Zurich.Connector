using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.App;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Tests.ServiceTests
{
    [TestClass]
    public class CosmosServiceTests
    {
        private Mock<ICosmosDocumentReader> _mockCosmosDocumentReader;
        private Mock<ICosmosDocumentWriter> _mockCosmosDocumentWriter;
        private IMapper _mapper;


        [TestInitialize]
        public void TestInitialize()
        {
            _mockCosmosDocumentReader = new Mock<ICosmosDocumentReader>();
            _mockCosmosDocumentWriter = new Mock<ICosmosDocumentWriter>();
            var mapConfig = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new ServiceMappingRegistrar());
            });
            _mapper = mapConfig.CreateMapper();

        }

        #region Data Setup
        private IEnumerable<ConnectorDocument> SetupConnectors()
        {
            return new List<ConnectorDocument>()
            {
                new ConnectorDocument()
                {
                    Id = "1",
                    info = new ConnectorInfo()
                    {
                        title = "title1",
                        dataSourceId = "100",
                        description ="desc1"
                    }
                },
                new ConnectorDocument()
                {
                    Id = "2",
                    info = new ConnectorInfo()
                    {
                        title = "title2",
                        dataSourceId = "102",
                        description ="desc2"
                    }
                },
                new ConnectorDocument()
                {
                    Id = "3",
                    info = new ConnectorInfo()
                    {
                        title = "title3",
                        dataSourceId = "103",
                        description ="desc3"
                    }
                }
            };
        }
        private IEnumerable<DataSourceDocument> SetupDataSources()
        {
            return new List<DataSourceDocument>()
            {
                new DataSourceDocument()
                {
                    Id = "101",
                    description = "data source doc 1"
                },
                new DataSourceDocument()
                {
                    Id = "102",
                    description = "data source doc 2"
                },
                new DataSourceDocument()
                {
                    Id = "103",
                    description = "data source doc 3"
                }
            };
        }
        #endregion


        [TestMethod]
        public async Task CallGetConnectors()
        {
            //Arrange
            var testConnectors = SetupConnectors();
            var testConnectorsList = testConnectors.ToList();
            _mockCosmosDocumentReader.Setup(x => x.GetConnectorDocuments(null)).Returns(Task.FromResult(testConnectors));
            var cosmostService = new CosmosService(_mockCosmosDocumentReader.Object, _mockCosmosDocumentWriter.Object, null,_mapper, null);

            //Act
            var connectors = (await cosmostService.GetConnectors()).ToList();

            //Assert
            _mockCosmosDocumentReader.Verify(x => x.GetConnectorDocuments(null), Times.Once());
            Assert.IsNotNull(connectors);
            Assert.AreEqual(SetupConnectors().Count(), connectors.Count());
            Assert.AreEqual(testConnectorsList[0].Id, connectors[0].Id);
        }

        [TestMethod]
        public async Task CallGetConnector()
        {
            //Arrange
            var testId = "3";
            var testConnector = SetupConnectors().Where(x => x.Id == testId).FirstOrDefault();
            _mockCosmosDocumentReader.Setup(x => x.GetConnectorDocument(testId)).Returns(Task.FromResult(testConnector));
            var cosmostService = new CosmosService(_mockCosmosDocumentReader.Object, _mockCosmosDocumentWriter.Object, null, _mapper, null);

            //Act
            var connector = await cosmostService.GetConnector(testId);

            //Assert
            _mockCosmosDocumentReader.Verify(x => x.GetConnectorDocument(testId), Times.Once());
            Assert.IsNotNull(connector);
            Assert.AreEqual(testConnector.Id, connector.Id);
            Assert.AreEqual(testConnector.info.title, connector.Info.Title);
        }

        [TestMethod]
        public async Task CallGetConnectorsByIds()
        {
            //Arrange
            var testIds = new List<string> { "1", "3" };
            var testConnectors = SetupConnectors().Where(x => testIds.Contains(x.Id));
            Expression<Func<ConnectorDocument, bool>> condition = connectors => testIds.Contains(connectors.Id);
            _mockCosmosDocumentReader.Setup(x => x.GetConnectorDocuments(condition)).Returns(Task.FromResult(testConnectors));
            var cosmostService = new CosmosService(_mockCosmosDocumentReader.Object, _mockCosmosDocumentWriter.Object, null, _mapper, null);

            //Act
            var connectors = (await cosmostService.GetConnectors(false, condition)).ToList();

            //Assert
            _mockCosmosDocumentReader.Verify(x => x.GetConnectorDocuments(condition), Times.Once());
            Assert.IsNotNull(connectors);
            Assert.AreEqual(testConnectors.Count(), connectors.Count());
            Assert.AreEqual(testConnectors.ToList()[0].Id, connectors[0].Id);
            Assert.AreEqual(testConnectors.ToList()[0].info.title, connectors[0].Info.Title);
        }

        [TestMethod]
        public async Task CallGetDataSources()
        {
            //Arrange
            var testDataSources = SetupDataSources();
            var testDataSourcesList = SetupDataSources().ToList();
            _mockCosmosDocumentReader.Setup(x => x.GetDataSourceDocuments(null)).Returns(Task.FromResult(testDataSources));
            var cosmostService = new CosmosService(_mockCosmosDocumentReader.Object, _mockCosmosDocumentWriter.Object, null, _mapper, null);

            //Act
            var dataSources = (await cosmostService.GetDataSources()).ToList();

            //Assert
            _mockCosmosDocumentReader.Verify(x => x.GetDataSourceDocuments(null), Times.Once());
            Assert.IsNotNull(dataSources);
            Assert.AreEqual(SetupDataSources().Count(), dataSources.Count());
            Assert.AreEqual(testDataSourcesList[0].Id, dataSources[0].Id);
        }

        [TestMethod]
        public async Task CallGetDataSource()
        {
            //Arrange
            var testId = "101";
            var testDataSource = SetupDataSources().Where(p => p.Id == testId).FirstOrDefault();
            _mockCosmosDocumentReader.Setup(x => x.GetDataSourceDocument(testId)).Returns(Task.FromResult(testDataSource));
            var cosmostService = new CosmosService(_mockCosmosDocumentReader.Object, _mockCosmosDocumentWriter.Object, null, _mapper, null);

            //Act
            var dataSource = await cosmostService.GetDataSource(testId);

            //Assert
            _mockCosmosDocumentReader.Verify(x => x.GetDataSourceDocument(testId), Times.Once());
            Assert.IsNotNull(dataSource);
            Assert.AreEqual(testDataSource.Id, dataSource.Id);
            Assert.AreEqual(testDataSource.description, dataSource.Description);

        }

        [TestMethod]
        public async Task CallStoreConnector()
        {
            //Arrange
            var testId = "1";
            var testConnector = SetupConnectors().Where(p => p.Id == testId).FirstOrDefault();
            var cosmostService = new CosmosService(_mockCosmosDocumentReader.Object, _mockCosmosDocumentWriter.Object, null, _mapper, null);

            //Act
            await cosmostService.StoreConnector(testConnector);

            //Assert
            _mockCosmosDocumentWriter.Verify(x => x.StoreConnector(testConnector), Times.Once());
        }

        [TestMethod]
        public async Task CallStoreDataSource()
        {
            //Arrange
            var testId = "101";
            var testDataSource = SetupDataSources().Where(p => p.Id == testId).FirstOrDefault();
            var cosmostService = new CosmosService(_mockCosmosDocumentReader.Object, _mockCosmosDocumentWriter.Object, null, _mapper, null);

            //Act
            await cosmostService.StoreDataSource(testDataSource);

            //Assert
            _mockCosmosDocumentWriter.Verify(x => x.StoreDataSoruce(testDataSource), Times.Once());
        }


    }
}
