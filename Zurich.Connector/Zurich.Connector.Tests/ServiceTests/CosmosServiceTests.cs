using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common.Repositories.Cosmos;
using Zurich.Connector.App;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;

namespace Zurich.Connector.Tests.ServiceTests
{
    [TestClass]
    public class CosmosServiceTests
    {
        private Mock<ConnectorCosmosContext> _mockCosmosClientStore;
        private IMapper _mapper;
       

        [TestInitialize]
        public void TestInitialize()
        {
            _mockCosmosClientStore = new Mock<ConnectorCosmosContext>(null, null);
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
                    Alias = "A.B.C",
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
                    Alias = "X.Y.Z",
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
                    Alias = "P.Q.R",
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
        private IEnumerable<ConnectorRegistrationDocument> SetupConnectorRegistration()
        {
            return new List<ConnectorRegistrationDocument>()
            {
                new ConnectorRegistrationDocument()
                {
                    Id = "101",
                    ConnectorId = "1",
                   
                },
                new ConnectorRegistrationDocument()
                {
                    Id = "102",
                   ConnectorId = "3",
                    
                }
            };
        }
        private IEnumerable<ConnectorRegistrationDocument> SetupGetUserRegistration()
        {
            return new List<ConnectorRegistrationDocument>()
            {
                new ConnectorRegistrationDocument()
                {
                    Id = "140",
                    ConnectorId = "140",
                },
                new ConnectorRegistrationDocument()
                {
                    Id = "141",
                   ConnectorId = "141",
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
            _mockCosmosClientStore.Setup(x => x.GetDocuments<ConnectorDocument>(CosmosConstants.ConnectorContainerId, CosmosConstants.ConnectorPartitionKey, null))
                                    .Returns((testConnectors));
            var cosmostService = new CosmosService(_mockCosmosClientStore.Object, _mapper, null);

            //Act
            var connectors = (await cosmostService.GetConnectors()).ToList();

            //Assert
            _mockCosmosClientStore.Verify(x => x.GetDocuments<ConnectorDocument>(CosmosConstants.ConnectorContainerId, CosmosConstants.ConnectorPartitionKey, null),
                                            Times.Once());
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
            _mockCosmosClientStore.Setup(x => x.GetDocument<ConnectorDocument>(CosmosConstants.ConnectorContainerId, testId, CosmosConstants.ConnectorPartitionKey))
                                        .Returns(Task.FromResult(testConnector));
            var cosmostService = new CosmosService(_mockCosmosClientStore.Object, _mapper, null);

            //Act
            var connector = await cosmostService.GetConnector(testId);

            //Assert
            _mockCosmosClientStore.Verify(x => x.GetDocument<ConnectorDocument>(CosmosConstants.ConnectorContainerId, testId, CosmosConstants.ConnectorPartitionKey), 
                                        Times.Once());
            Assert.IsNotNull(connector);
            Assert.AreEqual(testConnector.Id, connector.Id);
            Assert.AreEqual(testConnector.info.title, connector.Info.Title);
        }

        [TestMethod]
        public async Task CallGetConnectorByAlias()
        {
            //Arrange
            var testAlias = "a.b.c";
            var testConnectors = SetupConnectors();

            _mockCosmosClientStore.Setup(x => x.GetDocuments(CosmosConstants.ConnectorContainerId, CosmosConstants.ConnectorPartitionKey, It.IsAny<Expression<Func<ConnectorDocument, bool>>>()))
                                    .Returns(testConnectors);
            
            var cosmosService = new CosmosService(_mockCosmosClientStore.Object, _mapper, null);

            //Act
            var connector = await cosmosService.GetConnectorByAlias(testAlias);

            //Assert
            Assert.IsNotNull(connector);
            Assert.AreEqual(connector.Alias, testAlias, true);
        }

        [TestMethod]
        public async Task CallGetConnectorsByIds()
        {
            //Arrange
            var testIds = new List<string> { "1", "3" };
            var testConnectors = SetupConnectors().Where(x => testIds.Contains(x.Id));
            Expression<Func<ConnectorDocument, bool>> condition = connectors => testIds.Contains(connectors.Id);
            _mockCosmosClientStore.Setup(x => x.GetDocuments(CosmosConstants.ConnectorContainerId, CosmosConstants.ConnectorPartitionKey, condition))
                                    .Returns(testConnectors);
            var cosmostService = new CosmosService(_mockCosmosClientStore.Object, _mapper, null);

            //Act
            var connectors = (await cosmostService.GetConnectors(false, condition)).ToList();

            //Assert
            _mockCosmosClientStore.Verify(x => x.GetDocuments(CosmosConstants.ConnectorContainerId, CosmosConstants.ConnectorPartitionKey, condition), Times.Once());
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
            _mockCosmosClientStore.Setup(x => x.GetDocuments<DataSourceDocument>(CosmosConstants.DataSourceContainerId, CosmosConstants.DataSourcePartitionKey, null))
                                .Returns(testDataSources);
            var cosmostService = new CosmosService(_mockCosmosClientStore.Object, _mapper, null);

            //Act
            var dataSources = (await cosmostService.GetDataSources()).ToList();

            //Assert
            _mockCosmosClientStore.Verify(x => x.GetDocuments<DataSourceDocument>(CosmosConstants.DataSourceContainerId, CosmosConstants.DataSourcePartitionKey, null), 
                                    Times.Once());
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
            _mockCosmosClientStore.Setup(x => x.GetDocument<DataSourceDocument>
                                        (CosmosConstants.DataSourceContainerId, testId, CosmosConstants.DataSourcePartitionKey))
                                        .Returns(Task.FromResult(testDataSource));
            var cosmostService = new CosmosService(_mockCosmosClientStore.Object, _mapper, null);

            //Act
            var dataSource = await cosmostService.GetDataSource(testId);

            //Assert
            _mockCosmosClientStore.Verify(x => x.GetDocument<DataSourceDocument>
                                        (CosmosConstants.DataSourceContainerId, testId, CosmosConstants.DataSourcePartitionKey), Times.Once());
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
            var cosmostService = new CosmosService(_mockCosmosClientStore.Object, _mapper, null);

            //Act
            await cosmostService.StoreConnector(testConnector);

            //Assert
            _mockCosmosClientStore.Verify(x => x.UpsertDocument(testConnector, CosmosConstants.ConnectorContainerId), Times.Once());
        }

        [TestMethod]
        public async Task CallStoreDataSource()
        {
            //Arrange
            var testId = "101";
            var testDataSource = SetupDataSources().Where(p => p.Id == testId).FirstOrDefault();
            var cosmostService = new CosmosService(_mockCosmosClientStore.Object, _mapper, null);

            //Act
            await cosmostService.StoreDataSource(testDataSource);

            //Assert
            _mockCosmosClientStore.Verify(x => x.UpsertDocument(testDataSource, CosmosConstants.DataSourceContainerId), Times.Once());
        }

        [TestMethod]
        public async Task CallStoreConnectorRegistration()
        {
            //Arrange
            var testId = "101";
            var testDataSource = SetupConnectorRegistration().Where(p => p.Id == testId).FirstOrDefault();
            var cosmostService = new CosmosService(_mockCosmosClientStore.Object, _mapper, null);

            //Act
            await cosmostService.StoreConnectorRegistration(testDataSource);

            //Assert
            _mockCosmosClientStore.Verify(x => x.UpsertDocument(testDataSource, CosmosConstants.ConnectorRegistrationContainerId), Times.Once());
        }
        [TestMethod]
        public async Task callGetUserRegistration()
        {
            //Arrange
            var testId = "140";
            string UserId = "55e7a5d2-2134-4828-a2cd-2c4284ec11b9";
            var testConnectors = SetupGetUserRegistration();
            _mockCosmosClientStore.Setup(x => x.GetDocument<ConnectorRegistrationDocument>(CosmosConstants.ConnectorRegistrationContainerId, testId, UserId));
                                    
            var cosmosService = new CosmosService(_mockCosmosClientStore.Object, _mapper, null);

            //Act
            var connectors = await cosmosService.GetUserRegistration(testId, UserId);
            //Assert
            _mockCosmosClientStore.Verify(x => x.GetDocument<ConnectorRegistrationDocument>(CosmosConstants.ConnectorRegistrationContainerId, testId, UserId),
                                            Times.Once());
            
        }
    }
}
