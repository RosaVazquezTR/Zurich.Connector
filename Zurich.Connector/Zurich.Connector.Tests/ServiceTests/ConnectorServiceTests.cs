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
						Auth = new DataMappingAuth() { Type = AuthType.OAuth2 },
						EntityType = Data.Model.EntityType.History
					}
				},
				{
					new DataMappingConnection()
					{
						AppCode = "testApp2",
						Auth = new DataMappingAuth() { Type = AuthType.TransferToken },
						EntityType = Data.Model.EntityType.Document
					}
				},
				{
					new DataMappingConnection()
					{
						AppCode = "testApp2",
						Auth = new DataMappingAuth() { Type = AuthType.OAuth2 },
						EntityType = Data.Model.EntityType.History
					}
				}
			};
		}
		#endregion

		[TestMethod]
		public async Task CallGetConnectors()
		{
			// ARRANGE
			var testConnections = MockConnectorData.SetupConnectorModel();
			var testConnectionsList = MockConnectorData.SetupConnectorModel().ToList();
			var testDataSourceIds = testConnections.Select(t => t.Info.DataSourceId).Distinct().ToList();
			var testDataSources = MockConnectorData.SetupDataSourceModel().Where(t => testDataSourceIds.Contains(t.Id));
			var testDataSourcesList = testDataSources.ToList();
			ConnectorFilterModel filters = new ConnectorFilterModel();
			_mockCosmosService.Setup(x => x.GetConnectors(true, null)).Returns(Task.FromResult(testConnections));

			Expression<Func<DataSourceDocument, bool>> dsCondition = dataSources => testDataSourceIds.Contains(dataSources.Id);
			_mockCosmosService.Setup(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>())).Returns(Task.FromResult(testDataSources));

			ConnectorService service = new ConnectorService(_mockDataMapping.Object, _mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object);

			// ACT
			var connectors = await service.GetConnectors(filters);

			// ASSERT
			_mockCosmosService.Verify(x => x.GetConnectors(true, null), Times.Once());
			Assert.IsNotNull(connectors);
			Assert.AreEqual(MockConnectorData.SetupConnectorModel().ToList().Count, connectors.Count);
			Assert.AreEqual(testConnectionsList[0].Id, connectors[0].Id);
			Assert.AreEqual(testConnectionsList[1].Info.Title, connectors[1].Info.Title);
			var testName = testDataSourcesList.Where(t => t.Id == connectors[0].Info.DataSourceId).Select(t => t.Name).First();
			Assert.AreEqual(testName, connectors[0].DataSource.Name);
		}

		[TestMethod]
		public async Task CallMapQueryParametersFromDB()
        {
			// ARRANGE
			var cdmQueryParameters = new Dictionary<string, string>() { { "Offset", "1" }, {"ResultSize", "10" } };
			var connector = MockConnectorData.SetupConnectorModel().Where(t => t.Id == "1").FirstOrDefault();

			ConnectorService service = new ConnectorService(_mockDataMapping.Object, _mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object);

			// ACT
			var mappedResult = service.MapQueryParametersFromDB(cdmQueryParameters, connector);

			// ASSERT
			Assert.AreEqual(mappedResult["searchTerm"], "*");
			Assert.AreEqual(mappedResult["resultsStartIndex"], "1");
			Assert.AreEqual(mappedResult["resultsCount"], "10");
		}

		[TestMethod]
		public async Task CallMapQueryParametersFromDBWithZeroOffsetBasedPagination()
		{
			// ARRANGE
			var cdmQueryParameters = new Dictionary<string, string>() { { "Offset", "1" } };
			var connector = MockConnectorData.SetupConnectorModel().Where(t => t.Id == "2").FirstOrDefault();

			ConnectorService service = new ConnectorService(_mockDataMapping.Object, _mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object);

			// ACT
			var mappedResult = service.MapQueryParametersFromDB(cdmQueryParameters, connector);

			// ASSERT
			Assert.AreEqual(mappedResult["searchTerm"], "*");
			Assert.AreEqual(mappedResult["resultsStartIndex"], "0");
			Assert.AreEqual(mappedResult["resultsCount"], "25");
		}

		[TestMethod]
		public async Task CallGetConnectorsWithFilters()
		{
			// ARRANGE
			var testDataSourceId = "11";
			var testEntityType = EntityType.Search;
			var testDataSourceIds = new String[] { testDataSourceId };
			var testConnections = MockConnectorData.SetupConnectorModel().Where(t => testDataSourceIds.Contains(t.Info.DataSourceId));
			var testDataSources = MockConnectorData.SetupDataSourceModel().Where(t => testDataSourceIds.Contains(t.Id));
			ConnectorFilterModel filters = new ConnectorFilterModel()
			{
				DataSources = new List<string>() { testDataSourceId },
				EntityTypes = new List<EntityType>() { testEntityType }
			};

			var entityTypeFilter = filters.EntityTypes.Select(t => t.ToString());
			Expression<Func<ConnectorDocument, bool>> condition = connector => entityTypeFilter.Contains(connector.info.entityType.ToString());
			condition = connector => filters.DataSources.Contains(connector.info.dataSourceId);

			Expression<Func<DataSourceDocument, bool>> dsCondition = dataSources => testDataSourceIds.Contains(dataSources.Id);


			_mockCosmosService.Setup(x => x.GetConnectors(true, It.IsAny<Expression<Func<ConnectorDocument, bool>>>())).Returns(Task.FromResult(testConnections));
			_mockCosmosService.Setup(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>())).Returns(Task.FromResult(testDataSources));

			ConnectorService service = new ConnectorService(_mockDataMapping.Object, _mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object);

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

			Expression<Func<ConnectorDocument, bool>> condition = connector => filters.DataSources.Contains(connector.info.dataSourceId);
			Expression<Func<DataSourceDocument, bool>> dsCondition = dataSources => testDataSourceIds.Contains(dataSources.Id);

			_mockCosmosService.Setup(x => x.GetConnectors(true, It.IsAny<Expression<Func<ConnectorDocument, bool>>>())).Returns(Task.FromResult(testConnections));
			_mockCosmosService.Setup(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>())).Returns(Task.FromResult(testDataSources));

			ConnectorService service = new ConnectorService(_mockDataMapping.Object, _mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object);

			// ACT
			var connectors = await service.GetConnectors(filters);

			// ASSERT
			_mockCosmosService.Verify(x => x.GetConnectors(true, It.IsAny<Expression<Func<ConnectorDocument, bool>>>()), Times.Once());
			Assert.IsNotNull(connectors);
			Assert.AreEqual(1, connectors.Count);
			Assert.AreEqual(connectors[0].Info.DataSourceId, testDataSourceId);
		}

	}
}
