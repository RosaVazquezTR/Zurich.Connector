using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Services;

namespace Zurich.Connector.Tests.ServiceTests
{
	[TestClass]
	public class ConnectorServiceTests
    {
		private Mock<IDataMapping> _mockDataMapping;
		private Mock<IDataMappingFactory> _mockDataMappingFactory;
		private Mock<IDataMappingRepository> _mockDataMappingRepo;

		[TestInitialize]
		public void TestInitialize()
		{
			_mockDataMapping = new Mock<IDataMapping>();
			_mockDataMappingFactory = new Mock<IDataMappingFactory>();
			_mockDataMappingRepo = new Mock<IDataMappingRepository>();
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
						Auth = new DataMappingAuth() { Type = AuthType.OAuth },
						EntityType = EntityType.History
					}
				},
				{
					new DataMappingConnection()
					{
						AppCode = "testApp2",
						Auth = new DataMappingAuth() { Type = AuthType.TransferToken },
						EntityType = EntityType.Document
					}
				},
				{
					new DataMappingConnection()
					{
						AppCode = "testApp2",
						Auth = new DataMappingAuth() { Type = AuthType.OAuth },
						EntityType = EntityType.History
					}
				}
			};
		}

        #endregion

        [TestMethod]
		public async Task CallGetConnectors()
		{
			// ARRANGE
			var testConnections = SetupConnections();
			ConnectorFilterModel filters = new ConnectorFilterModel();
			_mockDataMappingRepo.Setup(x => x.GetConnectors()).Returns(Task.FromResult(testConnections));
			
			ConnectorService service = new ConnectorService(_mockDataMapping.Object, _mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null,null);

			// ACT
			var connectors = await service.GetConnectors(filters);

			// ASSERT
			_mockDataMappingRepo.Verify(x => x.GetConnectors(), Times.Once());
			Assert.IsNotNull(connectors);
			Assert.AreEqual(SetupConnections().Count, connectors.Count);
			Assert.AreEqual(testConnections[0].AppCode, connectors[0].AppCode);
			Assert.AreEqual(testConnections[2].Auth.Type, connectors[2].Auth.Type);
		}

		[TestMethod]
		public async Task CallGetConnectorsWithFilters()
		{
			// ARRANGE
			var testConnections = SetupConnections();
			ConnectorFilterModel filters = new ConnectorFilterModel()
			{
				DataSources = new List<string>() { "testApp2", "testApp1" },
				EntityTypes = new List<EntityType>() { EntityType.Document }
			};
			_mockDataMappingRepo.Setup(x => x.GetConnectors()).Returns(Task.FromResult(testConnections));

			ConnectorService service = new ConnectorService(_mockDataMapping.Object, _mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null,null);

			// ACT
			var connectors = await service.GetConnectors(filters);

			// ASSERT
			_mockDataMappingRepo.Verify(x => x.GetConnectors(), Times.Once());
			Assert.IsNotNull(connectors);
			Assert.AreEqual(1, connectors.Count);
			Assert.AreEqual(connectors[0].AppCode, "testApp2");
			Assert.AreEqual(connectors[0].EntityType, EntityType.Document);
		}

		[TestMethod]
		public async Task CallGetConnectorsWithAuthFilter()
		{
			// ARRANGE
			var testConnections = SetupConnections();
			ConnectorFilterModel filters = new ConnectorFilterModel()
			{
				AuthTypes = new List<AuthType>() { AuthType.OAuth }
			};
			_mockDataMappingRepo.Setup(x => x.GetConnectors()).Returns(Task.FromResult(testConnections));

			ConnectorService service = new ConnectorService(_mockDataMapping.Object, _mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null,null);

			// ACT
			var connectors = await service.GetConnectors(filters);

			// ASSERT
			_mockDataMappingRepo.Verify(x => x.GetConnectors(), Times.Once());
			Assert.IsNotNull(connectors);
			Assert.AreEqual(2, connectors.Count);
			Assert.AreEqual(connectors[0].Auth.Type, AuthType.OAuth);
			Assert.AreEqual(connectors[1].Auth.Type, AuthType.OAuth);
		}

	}
}
