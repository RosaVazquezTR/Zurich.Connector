using AutoMapper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Connector.App;
using Zurich.Connector.App.Services;
using Microsoft.AspNetCore.Http;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;
using System.Linq.Expressions;
using Zurich.Connector.App.Model;
using Zurich.Connector.Tests.Common;
using Zurich.Connector.App.Services.DataSources;
using Zurich.Common.Models.Connectors;
using Zurich.Connector.App.Enum;
using Zurich.Connector.Data.Factories;

namespace Zurich.Connector.Tests.ServiceTests
{
    [TestClass]
	public class ConnectorServiceTests
	{
		private Mock<IDataMapping> _mockDataMapping;
		private Mock<IDataMappingFactory> _mockDataMappingFactory;
		private Mock<IDataMappingRepository> _mockDataMappingRepo;
		private Mock<IQueryCollection> _mockQueryCollection;
		private Mock<ICosmosService> _mockCosmosService;
		private IMapper _mapper;
		private Mock<IDataMappingService> _mockdataMappingService;
		private Mock<IConnectorDataSourceOperationsFactory> _mockDataSourceOperationsFactory;
		private Mock<IRegistrationService> _mockRegistrationService;

		[TestInitialize]
		public void TestInitialize()
		{
			_mockDataMapping = new Mock<IDataMapping>();
			_mockDataMappingFactory = new Mock<IDataMappingFactory>();
			_mockDataMappingRepo = new Mock<IDataMappingRepository>();

			var mapConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile(new ServiceMappingRegistrar());
			});
			_mapper = mapConfig.CreateMapper();
			_mockQueryCollection = new Mock<IQueryCollection>();
			_mockCosmosService = new Mock<ICosmosService>();
			_mockdataMappingService = new Mock<IDataMappingService>();
			_mockDataSourceOperationsFactory = new Mock<IConnectorDataSourceOperationsFactory>();
			_mockRegistrationService = new Mock<IRegistrationService>();
		}

		#region Data Setup
		private List<DataMappingConnection> SetupConnections()
		{
			return new List<DataMappingConnection>()
			{
				{
					new DataMappingConnection()
					{
						AppCode = "testApp1",
						Auth = new DataMappingAuth() { Type = Data.Model.AuthType.OAuth2 },
						EntityType = Data.Model.ConnectorEntityType.History
					}
				},
				{
					new DataMappingConnection()
					{
						AppCode = "testApp2",
						Auth = new DataMappingAuth() { Type = Data.Model.AuthType.TransferToken },
						EntityType = Data.Model.ConnectorEntityType.Document
					}
				},
				{
					new DataMappingConnection()
					{
						AppCode = "testApp2",
						Auth = new DataMappingAuth() { Type = Data.Model.AuthType.OAuth2 },
						EntityType = Data.Model.ConnectorEntityType.History
					}
				}
			};
		}
		#endregion

		[TestMethod]
		public async Task CallGetConnectors()
		{
			// ARRANGE
			var registeredDataSources = new List<string>() { "DataSource22" };
			var testConnections = MockConnectorData.SetupConnectorModel();
			var testConnectionsList = MockConnectorData.SetupConnectorModel().ToList();

			var testDataSourceIds = testConnections.Select(t => t.Info.DataSourceId).Distinct().ToList();
			var testDataSources = MockConnectorData.SetupDataSourceModel().Where(t => testDataSourceIds.Contains(t.Id));
			var testDataSourcesList = testDataSources.ToList();
			ConnectorFilterModel filters = new ConnectorFilterModel();

			_mockCosmosService.Setup(x => x.GetConnectors(true, It.IsAny<Expression<Func<ConnectorDocument, bool>>>())).Returns(Task.FromResult(testConnections));
			_mockRegistrationService.Setup(x => x.GetUserDataSources()).Returns(Task.FromResult<IEnumerable<string>>(registeredDataSources));
			_mockCosmosService.Setup(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>())).Returns(Task.FromResult(testDataSources));

			ConnectorService service = new ConnectorService(null, _mockCosmosService.Object, _mockRegistrationService.Object);

			// ACT
			var connectors = await service.GetConnectors(filters);

			// ASSERT
			_mockCosmosService.Verify(x => x.GetConnectors(true, It.IsAny<Expression<Func<ConnectorDocument, bool>>>()), Times.Once());
			Assert.IsNotNull(connectors);
			Assert.AreEqual(MockConnectorData.SetupConnectorModel().ToList().Count, connectors.Count);
			Assert.AreEqual(testConnectionsList[0].Id, connectors[0].Id);
			Assert.AreEqual(testConnectionsList[1].Info.Title, connectors[1].Info.Title);
			var testName = testDataSourcesList.Where(t => t.Id == connectors[0].Info.DataSourceId).Select(t => t.Name).First();
			Assert.AreEqual(testName, connectors[0].DataSource.Name);
			Assert.AreEqual(App.Enum.RegistrationStatus.Registered, connectors.Find(x => x.DataSource.AppCode == registeredDataSources.First()).RegistrationStatus);
			Assert.AreEqual(App.Enum.RegistrationStatus.NotRegistered, connectors.Find(x => !registeredDataSources.Contains(x.DataSource.AppCode)).RegistrationStatus);
		}

		[TestMethod]
		public async Task CallGetConnectorsWithFilters()
		{
			// ARRANGE
			var testDataSourceId = "11";
			var testEntityType = Data.Model.ConnectorEntityType.Search;
			var testDataSourceIds = new String[] { testDataSourceId };
			var testConnections = MockConnectorData.SetupConnectorModel().Where(t => testDataSourceIds.Contains(t.Info.DataSourceId));
			var testDataSources = MockConnectorData.SetupDataSourceModel().Where(t => testDataSourceIds.Contains(t.Id));
			ConnectorFilterModel filters = new ConnectorFilterModel()
			{
				DataSources = new List<string>() { testDataSourceId },
				EntityTypes = new List<Zurich.Common.Models.Connectors.EntityType>() { (Zurich.Common.Models.Connectors.EntityType)testEntityType }
			};

			var entityTypeFilter = filters.EntityTypes.Select(t => t.ToString());
			Expression<Func<ConnectorDocument, bool>> condition = connector => entityTypeFilter.Contains(connector.Info.EntityType.ToString());
			condition = connector => filters.DataSources.Contains(connector.Info.DataSourceId);

			Expression<Func<DataSourceDocument, bool>> dsCondition = dataSources => testDataSourceIds.Contains(dataSources.Id);


			_mockCosmosService.Setup(x => x.GetConnectors(true, It.IsAny<Expression<Func<ConnectorDocument, bool>>>())).Returns(Task.FromResult(testConnections));
			_mockCosmosService.Setup(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>())).Returns(Task.FromResult(testDataSources));

			ConnectorService service = new ConnectorService(null, _mockCosmosService.Object, _mockRegistrationService.Object);

			// ACT
			var connectors = await service.GetConnectors(filters);

			// ASSERT
			_mockCosmosService.Verify(x => x.GetConnectors(true, It.IsAny<Expression<Func<ConnectorDocument, bool>>>()), Times.Once());
			Assert.IsNotNull(connectors);
			Assert.AreEqual(1, connectors.Count);
			Assert.AreEqual(connectors[0].Info.DataSourceId, testDataSourceId);
			Assert.AreEqual(connectors[0].Info.EntityType, testEntityType);
		}

		[TestMethod]
		public async Task CallGetConnectorsWithDataSourceFilter()
		{
			// ARRANGE
			var testDataSourceId = "11";
			var testDataSourceIds = new String[] { testDataSourceId };

			var testConnections = MockConnectorData.SetupConnectorModel().Where(t => testDataSourceIds.Contains(t.Info.DataSourceId));
			var testDataSources = MockConnectorData.SetupDataSourceModel().Where(t => testDataSourceIds.Contains(t.Id));
			ConnectorFilterModel filters = new ConnectorFilterModel()
			{
				DataSources = new List<string>() { testDataSourceId }
			};

			Expression<Func<ConnectorDocument, bool>> condition = connector => filters.DataSources.Contains(connector.Info.DataSourceId);
			Expression<Func<DataSourceDocument, bool>> dsCondition = dataSources => testDataSourceIds.Contains(dataSources.Id);

			_mockCosmosService.Setup(x => x.GetConnectors(true, It.IsAny<Expression<Func<ConnectorDocument, bool>>>())).Returns(Task.FromResult(testConnections));
			_mockCosmosService.Setup(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>())).Returns(Task.FromResult(testDataSources));

			ConnectorService service = new ConnectorService(null, _mockCosmosService.Object, _mockRegistrationService.Object);

			// ACT
			var connectors = await service.GetConnectors(filters);

			// ASSERT
			_mockCosmosService.Verify(x => x.GetConnectors(true, It.IsAny<Expression<Func<ConnectorDocument, bool>>>()), Times.Once());
			Assert.IsNotNull(connectors);
			Assert.AreEqual(1, connectors.Count);
			Assert.AreEqual(connectors[0].Info.DataSourceId, testDataSourceId);
		}

		[TestMethod]
		public async Task CallGetConnectorsWithIsRegisteredFilter()
		{
			// ARRANGE
			var registeredDataSources = new List<string>() { "DataSource22" };
			var testConnections = MockConnectorData.SetupConnectorModel();

			ConnectorFilterModel filters = new ConnectorFilterModel()
			{
				IsRegistered = true
			};


			_mockCosmosService.Setup(x => x.GetConnectors(true, It.IsAny<Expression<Func<ConnectorDocument, bool>>>())).Returns(Task.FromResult(testConnections));
			_mockRegistrationService.Setup(x => x.GetUserDataSources()).Returns(Task.FromResult<IEnumerable<string>>(registeredDataSources));

			ConnectorService service = new ConnectorService(null, _mockCosmosService.Object, _mockRegistrationService.Object);

			// ACT
			var connectors = await service.GetConnectors(filters);

			// ASSERT
			_mockCosmosService.Verify(x => x.GetConnectors(true, It.IsAny<Expression<Func<ConnectorDocument, bool>>>()), Times.Once());
			Assert.IsNotNull(connectors);
			Assert.AreEqual(2, connectors.Count);
			Assert.AreEqual(registeredDataSources.First(), connectors[0].DataSource.AppCode);
			connectors.ForEach(connector =>
			{
				Assert.AreEqual(App.Enum.RegistrationStatus.Registered, connector.RegistrationStatus);
			});
		}

		[TestMethod]
		public async Task CallGetConnectorsWithRegistrationModesFilter()
		{
			// ARRANGE
			var registeredConnectorIds = new List<string>() { "2", "4" };
			var testConnections = MockConnectorData.SetupConnectorModel().Where(t => registeredConnectorIds.Contains(t.Id));

			ConnectorFilterModel filters = new ConnectorFilterModel()
			{
				RegistrationModes = new List<Zurich.Common.Models.Connectors.RegistrationEntityMode>()
				{ Zurich.Common.Models.Connectors.RegistrationEntityMode.Automatic ,
				{ Zurich.Common.Models.Connectors.RegistrationEntityMode.Manual }
				}
			};

			_mockCosmosService.Setup(x => x.GetConnectors(true, It.IsAny<Expression<Func<ConnectorDocument, bool>>>())).Returns(Task.FromResult(testConnections));
			_mockRegistrationService.Setup(x => x.GetUserDataSources()).Returns(Task.FromResult<IEnumerable<string>>(registeredConnectorIds));

			ConnectorService service = new ConnectorService(null, _mockCosmosService.Object, _mockRegistrationService.Object);

			// ACT
			var connectors = await service.GetConnectors(filters);

			// ASSERT
			_mockCosmosService.Verify(x => x.GetConnectors(true, It.IsAny<Expression<Func<ConnectorDocument, bool>>>()), Times.Once());
			Assert.IsNotNull(connectors);
            Assert.AreEqual(2, connectors.Count);
            //Assert.AreEqual(MockConnectorData.SetupConnectorModel().ToList().Count, connectors.Count);
            Assert.AreEqual(registeredConnectorIds.First(), connectors[0].Id);
			Assert.AreEqual(Zurich.Common.Models.Connectors.RegistrationEntityMode.Manual, connectors[0].DataSource.RegistrationInfo.RegistrationMode);
			Assert.AreEqual(registeredConnectorIds.Last(), connectors[1].Id);
			Assert.AreEqual(Zurich.Common.Models.Connectors.RegistrationEntityMode.Automatic, connectors[1].DataSource.RegistrationInfo.RegistrationMode);


		}
	}
}
