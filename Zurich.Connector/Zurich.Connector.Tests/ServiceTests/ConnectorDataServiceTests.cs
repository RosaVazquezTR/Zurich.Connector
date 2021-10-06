using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Common.Services;
using Zurich.Connector.App;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Services;
using Zurich.Connector.App.Services.DataSources;
using Zurich.Connector.App.Utils;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Tests.Common;

namespace Zurich.Connector.Tests.ServiceTests
{
	[TestClass]
    public class ConnectorDataServiceTests
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

		[TestMethod]
		public async Task CallMapQueryParametersFromDB()
		{
			// ARRANGE
			var cdmQueryParameters = new Dictionary<string, string>() { { "Offset", "1" }, { "ResultSize", "10" } };
			var connector = MockConnectorData.SetupConnectorModel().Where(t => t.Id == "1").FirstOrDefault();

			ConnectorDataService service = new ConnectorDataService(_mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object);

			// ACT
			var mappedResult = service.MapQueryParametersFromDB(cdmQueryParameters, connector);

			// ASSERT
			Assert.AreEqual(mappedResult["searchTerm"], "*");
			Assert.AreEqual(mappedResult["resultsStartIndex"], "1");
			Assert.AreEqual(mappedResult["resultsCount"], "10");
		}


		[TestMethod]
		public void CallMapQueryParametersFromDBWithZeroOffsetBasedPagination()
		{
			TestPagination(pagination => pagination.IsZeroBasedOffset = true, 0);
		}

		[TestMethod]
		public void CallMapQueryParametersFromDBWithOneOffsetBasedPagination()
		{
			TestPagination(pagination => pagination.IsZeroBasedOffset = false, 1);
		}

		[TestMethod]
		public void CallMapQueryParametersFromDBWithNullPagination()
		{
			TestPagination(pagination => pagination.IsZeroBasedOffset = null, 0);
		}

		[TestMethod]
		public void CallMapQueryParametersFromDBWithDefaultToZeroOffsetBasedPagination()
		{
			TestPagination(pagination => pagination = null, 0);
		}

		private void TestPagination(Action<PaginationModel> arrangePagination, int expectedResult)
		{
			// ARRANGE
			var cdmQueryParameters = new Dictionary<string, string>() { { "Offset", "0" } };
			var connector = MockConnectorData.SetupConnectorModel().Where(t => t.Id == "2").FirstOrDefault();
			arrangePagination(connector.Pagination);

			ConnectorDataService service = new ConnectorDataService(_mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object);

			// ACT
			var mappedResult = service.MapQueryParametersFromDB(cdmQueryParameters, connector);

			// ASSERT
			Assert.AreEqual(mappedResult["searchTerm"], "*");
			Assert.AreEqual(mappedResult["resultsStartIndex"], expectedResult.ToString());
			Assert.AreEqual(mappedResult["resultsCount"], "25");
		}

		[TestMethod]
		public async Task CallMapQueryParametersFromDBUsingSortParameters()
		{
			// ARRANGE
			var cdmQueryParameters = new Dictionary<string, string>() { { "sort", "date" } };
			var connector = MockConnectorData.SetupConnectorModel().Where(t => t.Id == "3").FirstOrDefault();

			ConnectorDataService service = new ConnectorDataService(_mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object);

			// ACT
			var mappedResult = service.MapQueryParametersFromDB(cdmQueryParameters, connector);

			// ASSERT
			Assert.AreEqual(mappedResult["sortOrder"], "DATE");
		}

		[TestMethod]
		public async Task CallMapQueryParametersWithODataParams()
		{
			// ARRANGE
			var cdmQueryParameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) {
				{ "ResultSize", "5" },
				{ "doctypes", "video,word,excel" }
			};
			var connector = MockConnectorData.SetupConnectorModel().Where(t => t.Id == "4").FirstOrDefault();

			ConnectorDataService service = new ConnectorDataService(_mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object);

			// ACT
			var mappedResult = service.MapQueryParametersFromDB(cdmQueryParameters, connector);

			// ASSERT
			Assert.AreEqual("(resourceVisualization/type eq 'video' or resourceVisualization/type eq 'word' or resourceVisualization/type eq 'excel') and (referenece/type eq 'testValue')", mappedResult["$filter"]);
			Assert.AreEqual("5", mappedResult["$top"]);
		}

		[TestMethod]
		public async Task VerifyURLFormat()
		{
			// ARRANGE
			string appCode = "TestApp";

			ConnectorModel connectorDocument = new ConnectorModel()
			{
				Request = new ConnectorRequestModel()
				{ EndpointPath = "https://fakeaddress.thomsonreuters.com" }
			};
			connectorDocument.CDMMapping = new CDMMappingModel()
			{
				Structured = new List<CDMElementModel>() {
								{
									new CDMElementModel(){ Name="Name", ResponseElement ="name"}
								},
								{
									new CDMElementModel(){ Name="Id", ResponseElement ="id"}
								}
					}
			};

			DataSourceModel dataSourceDocument = new DataSourceModel()
			{
				AppCode = appCode,
				SecurityDefinition = new SecurityDefinitionModel()
				{
					DefaultSecurityDefinition = new SecurityDefinitionDetailsModel()
					{
						AuthorizationHeader = "differentAuthHeader"
					}
				}
			};
			connectorDocument.DataSource = dataSourceDocument;
			JToken fakeResult = new JObject();
			fakeResult["id"] = "241";

			_mockCosmosService.Setup(x => x.GetConnector("UserInfo", true)).Returns(Task.FromResult(connectorDocument));
			_mockDataMapping.Setup(x => x.GetAndMapResults<JToken>(It.IsAny<ConnectorDocument>(), null, null)).Returns(Task.FromResult(fakeResult));
			_mockDataMappingFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(_mockDataMapping.Object);

			ConnectorDataService service = new ConnectorDataService(_mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object);

			// ACT
			string newUrl = await service.UpdateUrl("/work/api/v2/customers/{UserInfo.id}/documents", "");

			// ASSERT
			Assert.IsNotNull(newUrl);
			Assert.AreEqual("/work/api/v2/customers/241/documents", newUrl);
		}

		[TestMethod]
		public async Task CallMapQueryParametersFromDBWithRequestSortingPropertiesAsNull()
		{
			// ARRANGE
			var cdmQueryParameters = new Dictionary<string, string>() { { "Offset", "1" }, { "ResultSize", "10" } };
			var connector = MockConnectorData.SetupConnectorModel_Version2().Where(t => t.Id == "1").FirstOrDefault();

			ConnectorDataService service = new ConnectorDataService(_mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object);

			// ACT
			var mappedResult = service.MapQueryParametersFromDB(cdmQueryParameters, connector);

			// ASSERT
			Assert.AreEqual(mappedResult["searchTerm"], "*");
			Assert.AreEqual(mappedResult["resultsStartIndex"], "1");
			Assert.AreEqual(mappedResult["resultsCount"], "10");
		}

		[TestMethod]
		public async Task CallMapQueryParametersFromDBWithRequestParametersAsNull()
		{
			// ARRANGE
			var cdmQueryParameters = new Dictionary<string, string>() { { "Offset", "1" }, { "ResultSize", "10" } };
			var connector = MockConnectorData.SetupConnectorModel_Version2().Where(t => t.Id == "2").FirstOrDefault();

			ConnectorDataService service = new ConnectorDataService(_mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object);

			// ACT
			var mappedResult = service.MapQueryParametersFromDB(cdmQueryParameters, connector);

			// ASSERT
			Assert.AreEqual(mappedResult.Count, 0);
		}

		[TestMethod]
		public async Task CallMapQueryParametersFromDBWithRequestParametersAndSortPropertiesAsNull()
		{
			// ARRANGE
			var cdmQueryParameters = new Dictionary<string, string>() { { "Offset", "1" }, { "ResultSize", "10" } };
			var connector = MockConnectorData.SetupConnectorModel_Version2().Where(t => t.Id == "3").FirstOrDefault();

			ConnectorDataService service = new ConnectorDataService(_mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object);

			// ACT
			var mappedResult = service.MapQueryParametersFromDB(cdmQueryParameters, connector);

			// ASSERT
			Assert.AreEqual(mappedResult.Count, 0);
		}

	}
}
