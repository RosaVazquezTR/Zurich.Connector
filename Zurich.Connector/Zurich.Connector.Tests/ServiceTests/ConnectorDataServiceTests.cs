using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Common.Services;
using Zurich.Connector.App;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Services;
using Zurich.Connector.App.Services.DataSources;
using Zurich.Connector.Data;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Tests.Common;
using CommonServices = Zurich.Common.Services;

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
		private Mock<IDataExtractionService> _mockDataExtractionService;
		private Mock<ILegalHomeAccessCheck> _mockLegalHomeAccess;
		private Mock<CommonServices.ITenantService> _mockTenantService;
		private Mock<IOAuthServices> _mockOAuthServices;
		private Mock<ILogger<Data.Services.ConnectorService>> _mockLogger;
		private Mock<IConfiguration> _mockConfiguration;

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
			_mockDataExtractionService = new Mock<IDataExtractionService>();
			_mockLegalHomeAccess = new Mock<ILegalHomeAccessCheck>();
			_mockTenantService = new Mock<ITenantService>();
			_mockOAuthServices = new Mock<IOAuthServices>();
			_mockLogger = new Mock<ILogger<Data.Services.ConnectorService>>();
			_mockConfiguration = new Mock<IConfiguration>();

		}

		[TestMethod]
		public async Task CallMapQueryParametersFromDB()
		{
			// ARRANGE
			var cdmQueryParameters = new Dictionary<string, string>() { { "Offset", "1" }, { "ResultSize", "10" } };
			var connector = MockConnectorData.SetupConnectorModel().Where(t => t.Id == "1").FirstOrDefault();

			ConnectorDataService service = new ConnectorDataService(_mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object, _mockDataExtractionService.Object, _mockLegalHomeAccess.Object, _mockTenantService.Object, _mockOAuthServices.Object, _mockConfiguration.Object);

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
			TestPaginationAsync(pagination => pagination.IsZeroBasedOffset = true, 0);
		}

		[TestMethod]
		public void CallMapQueryParametersFromDBWithOneOffsetBasedPagination()
		{
			TestPaginationAsync(pagination => pagination.IsZeroBasedOffset = false, 1);
		}

		[TestMethod]
		public void CallMapQueryParametersFromDBWithNullPagination()
		{
			TestPaginationAsync(pagination => pagination.IsZeroBasedOffset = null, 0);
		}

		[TestMethod]
		public void CallMapQueryParametersFromDBWithDefaultToZeroOffsetBasedPagination()
		{
			TestPaginationAsync(pagination => pagination = null, 0);
		}

		private async Task TestPaginationAsync(Action<PaginationModel> arrangePagination, int expectedResult)
		{
			// ARRANGE
			var cdmQueryParameters = new Dictionary<string, string>() { { "Offset", "0" } };
			var connector = MockConnectorData.SetupConnectorModel().Where(t => t.Id == "2").FirstOrDefault();
			arrangePagination(connector.Pagination);

			ConnectorDataService service = new ConnectorDataService(_mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object, _mockDataExtractionService.Object, _mockLegalHomeAccess.Object, _mockTenantService.Object, _mockOAuthServices.Object, _mockConfiguration.Object);

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
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object, _mockDataExtractionService.Object, _mockLegalHomeAccess.Object, _mockTenantService.Object, _mockOAuthServices.Object, _mockConfiguration.Object);

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
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object, _mockDataExtractionService.Object, _mockLegalHomeAccess.Object, _mockTenantService.Object, _mockOAuthServices.Object, _mockConfiguration.Object);

			// ACT
			var mappedResult = service.MapQueryParametersFromDB(cdmQueryParameters, connector);

			// ASSERT
			Assert.AreEqual("(resourceVisualization/type eq 'video' or resourceVisualization/type eq 'word' or resourceVisualization/type eq 'excel') and (referenece/type eq 'testValue')", mappedResult["$filter"]);
			Assert.AreEqual("5", mappedResult["$top"]);
		}
		[TestMethod]
		public async Task CallMapQueryParametersFromDBWithRequestSortingPropertiesAsNull()
		{
			// ARRANGE
			var cdmQueryParameters = new Dictionary<string, string>() { { "Offset", "1" }, { "ResultSize", "10" } };
			var connector = MockConnectorData.SetupConnectorModel_Version2().Where(t => t.Id == "1").FirstOrDefault();

			ConnectorDataService service = new ConnectorDataService(_mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object, _mockDataExtractionService.Object, _mockLegalHomeAccess.Object, _mockTenantService.Object, _mockOAuthServices.Object, _mockConfiguration.Object);

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
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object, _mockDataExtractionService.Object, _mockLegalHomeAccess.Object, _mockTenantService.Object, _mockOAuthServices.Object, _mockConfiguration.Object);

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
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object, _mockDataExtractionService.Object, _mockLegalHomeAccess.Object, _mockTenantService.Object, _mockOAuthServices.Object, _mockConfiguration.Object);

			// ACT
			var mappedResult = service.MapQueryParametersFromDB(cdmQueryParameters, connector);

			// ASSERT
			Assert.AreEqual(mappedResult.Count, 0);
		}

		[TestMethod]
		public async Task TestFiltersForResultSizeAsZero()
		{
			// ARRANGE
			Action<PaginationModel> arrangePagination = pagination => pagination = null;

			Dictionary<string, string> cdmQueryParameters = new Dictionary<string, string>() { { "Offset", "0" } , { "ResultSize", "0" } };
			var connector = MockConnectorData.SetupConnectorModel().Where(t => t.Id == "5").FirstOrDefault();
			var availableRegistrations = MockConnectorData.SetupAvailableUserRegistrations().ToList();
			arrangePagination(connector.Pagination);

			var fakeConfigValues = new Dictionary<string, string>
			{
				{AppSettings.InstanceLimit, "10"},
				{AppSettings.MaxRecordSizePerInstance, "1000"},
			};
			IConfiguration fakeConfig = Utility.CreateConfiguration(fakeConfigValues);

			ConnectorDataService service = new ConnectorDataService(_mockDataMappingFactory.Object, _mockDataMappingRepo.Object, null, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object, _mockDataExtractionService.Object, _mockLegalHomeAccess.Object, _mockTenantService.Object, _mockOAuthServices.Object, fakeConfig);

			
			_mockdataMappingService.Setup(x => x.RetrieveProductInformationMap(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(connector));
			_mockOAuthServices.Setup(x => x.GetUserRegistrations()).Returns(Task.FromResult(availableRegistrations));
			_mockdataMappingService.Setup(x => x.UpdateOffset(It.IsAny<string>(), It.IsAny<List<DataSourceInformation>>(), It.IsAny<Dictionary<string, string>>())).Returns(cdmQueryParameters);

			// ACT
			var mappedResult = await service.GetConnectorData("12", null, null, cdmQueryParameters, true);

			// ASSERT
			Assert.AreEqual( ((JObject)mappedResult)["filters"].Count() > 0, true );
			
		}

		[TestMethod]
		public async Task TestResultSizeNotPassed()
		{
			// ARRANGE
			Action<PaginationModel> arrangePagination = pagination => pagination = null;

			Dictionary<string, string> cdmQueryParameters = new Dictionary<string, string>() { };
			var connector = MockConnectorData.SetupConnectorModel().Where(t => t.Id == "5").FirstOrDefault();
			var availableRegistrations = MockConnectorData.SetupAvailableUserRegistrations().ToList();
			arrangePagination(connector.Pagination);
			var mockDataMappingImpl = new Mock<IDataMapping>();

			var fakeConfigValues = new Dictionary<string, string>
			{
				{AppSettings.InstanceLimit, "10"},
				{AppSettings.MaxRecordSizePerInstance, "1000"},
			};
			IConfiguration fakeConfig = Utility.CreateConfiguration(fakeConfigValues);

			_mockDataMappingFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockDataMappingImpl.Object);

			ConnectorDataService service = new ConnectorDataService(_mockDataMappingFactory.Object, _mockDataMappingRepo.Object, _mockLogger.Object, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object, _mockDataExtractionService.Object, _mockLegalHomeAccess.Object, _mockTenantService.Object, _mockOAuthServices.Object, fakeConfig);

			_mockdataMappingService.Setup(x => x.RetrieveProductInformationMap(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(connector));
			_mockOAuthServices.Setup(x => x.GetUserRegistrations()).Returns(Task.FromResult(availableRegistrations));
			_mockdataMappingService.Setup(x => x.UpdateOffset(It.IsAny<string>(), It.IsAny<List<DataSourceInformation>>(), It.IsAny<Dictionary<string, string>>())).Returns(cdmQueryParameters);

			// ACT
			var mappedResult = await service.GetConnectorData("12", null, null, cdmQueryParameters, false);

			// ASSERT
			mockDataMappingImpl.Verify(x => x.GetAndMapResults<dynamic>(It.IsAny<ConnectorDocument>(), It.IsAny<string>(), It.IsAny<NameValueCollection>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>()), Times.Once, "GetAndMapResults should be called once if no ResultSize");

		}

		[TestMethod]
		public async Task TestAvailableRegistrationseNotPassed()
		{
			// ARRANGE
			Action<PaginationModel> arrangePagination = pagination => pagination = null;

			Dictionary<string, string> cdmQueryParameters = new Dictionary<string, string>() { };
			var connector = MockConnectorData.SetupConnectorModel().Where(t => t.Id == "5").FirstOrDefault();
			List<DataSourceInformation> availableRegistrations = null;
			arrangePagination(connector.Pagination);
			var mockDataMappingImpl = new Mock<IDataMapping>();

			var fakeConfigValues = new Dictionary<string, string>
			{
				{AppSettings.InstanceLimit, "10"},
				{AppSettings.MaxRecordSizePerInstance, "1000"},
			};
			IConfiguration fakeConfig = Utility.CreateConfiguration(fakeConfigValues);

			_mockDataMappingFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockDataMappingImpl.Object);

			ConnectorDataService service = new ConnectorDataService(_mockDataMappingFactory.Object, _mockDataMappingRepo.Object, _mockLogger.Object, _mapper, _mockCosmosService.Object, _mockdataMappingService.Object,
				_mockDataSourceOperationsFactory.Object, _mockRegistrationService.Object, _mockDataExtractionService.Object, _mockLegalHomeAccess.Object, _mockTenantService.Object, _mockOAuthServices.Object, fakeConfig);

			_mockdataMappingService.Setup(x => x.RetrieveProductInformationMap(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(Task.FromResult(connector));
            _mockOAuthServices.Setup(x => x.GetUserRegistrations()).Returns(Task.FromResult(availableRegistrations));
            _mockdataMappingService.Setup(x => x.UpdateOffset(It.IsAny<string>(), It.IsAny<List<DataSourceInformation>>(), It.IsAny<Dictionary<string, string>>())).Returns(cdmQueryParameters);

            // ACT
            var mappedResult = await service.GetConnectorData("14", null, null, cdmQueryParameters, false);

			// ASSERT
			Assert.IsNull(mappedResult);

		}

	}
}
