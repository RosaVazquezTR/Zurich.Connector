using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Common.Exceptions;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Tests.Common;
using Zurich.Connector.Web;
using Zurich.Connector.Web.Controllers;

namespace Zurich.Connector.Tests.ControllerTests
{
    [TestClass]
	public class ConnectorTests
	{
		private Mock<IConnectorService> _mockConnectorservice;
		private Mock<IConnectorDataService> _mockConnectorDataService;
		private IMapper _mapper;
		private Mock<IMapper> _mockmapper;

		[TestInitialize]
		public void TestInitialize()
		{
			_mockConnectorservice = new Mock<IConnectorService>();
			_mockConnectorDataService = new Mock<IConnectorDataService>();

			var mapConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile(new MappingRegistrar());
			});
			_mapper = mapConfig.CreateMapper();
			_mockmapper = new Mock<IMapper>();
		}

		#region json Strings

		private const string TwoDocumentsListJson = @"
		{
			""results"": [{
				""author_description"": ""Ryan Hunecke"",
				""author"": ""RYAN.HUNECKE"",
				""class"": ""DOC"",
				""class_description"": ""Document""
			}, {
				""author_description"": ""Sally Sales"",
				""author"": ""ALEX.PRICE""
			}
		]}";

		private const string StaticFilterJson = @"
		{
			""results"":[{
				""name"": ""Commercial"",
				""description"": ""Commercial"",
				""isMultiselect"": ""true"",
				""requestParameter"": ""Commercial"",
				""filterlist"": [
					{
						""name"": ""Advertising and Marketing"",
						""id"": ""0-103-1114""

					},
					{
						""name"": ""Agency, Distribution & Franchising"",
						""id"": ""8-321-0007""

					},
							 ]
			}]
		}";

		//Intialization of test response data //
		string connectorid = "101";

		ConnectorsConfigResponseEntity Connector_Configuration = new ConnectorsConfigResponseEntity
		{
			Id = "101",
			AppCode = "Office",

			Api = new DataMappingApiRequest
			{
				AuthHeader = "id",
				MethodType = "Get",
				Hostname = "",
				Url = "https://graph.microsoft.com/v1.0/me"
			}

		};

		#endregion

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
						EntityType = Data.Model.ConnectorEntityType.History
					}
				},
				{
					new DataMappingConnection()
					{
						AppCode = "testApp2",
						Auth = new DataMappingAuth() { Type = AuthType.TransferToken },
						EntityType = Data.Model.ConnectorEntityType.History
					}
				},
				{
					new DataMappingConnection()
					{
						AppCode = "testApp2",
						Auth = new DataMappingAuth() { Type = AuthType.OAuth2 },
						EntityType = Data.Model.ConnectorEntityType.History
					}
				}

			};
		}
		private List<ConnectorModel> SetupMultiConnections()
		{
			return new List<ConnectorModel>()
			{
					new ConnectorModel()
					{
						Id="1",
						Filters= new List<ConnectorsFiltersModel>()
					},
					new ConnectorModel()
					{
						Id="101",
						Info=new ConnectorInfoModel()
						{
							Title="PracticleLaw",
							DataSourceId="1",
							Description="Test Connector",
							Version="1.0"

						},
						Request=new ConnectorRequestModel()
						{
							EndpointPath="/PracticalLaw/UK/Test",
							Method="Get",
							Parameters=new List<ConnectorRequestParameterModel>()
							{
							  new ConnectorRequestParameterModel()
							  {
								  Name="Test",
								  InClause="InClause",
								  Required=false,
								  CdmName="Test",
								  DefaultValue="Test",
								  Type="string"

							  }

							}

						},
						Response=new ConnectorResponseModel()
						{
							Schema=new ConnectorReponseSchemaModel()
							{
								Properties= new ConnectorReponsePropertiesModel()
								{
									Property="string"
								}
							}
						},
						Filters= new List<ConnectorsFiltersModel>()
						{
						}
						},
					new ConnectorModel()
					{
						Id="103",
						Info=new ConnectorInfoModel()
						{
							Title="Office",
							DataSourceId="1",
							Description="Test Connector",
							Version="1.0"

						},
						Request=new ConnectorRequestModel()
						{
							EndpointPath="/Office/Test",
							Method="Get",
							Parameters=new List<ConnectorRequestParameterModel>()
							{
							  new ConnectorRequestParameterModel()
							  {
								  Name="Test",
								  InClause="InClause",
								  Required=false,
								  CdmName="Test",
								  DefaultValue="Test",
								  Type="string"

							  }

							}

						},
						Response=new ConnectorResponseModel()
						{
							Schema=new ConnectorReponseSchemaModel()
							{
								Properties=new ConnectorReponsePropertiesModel()
								{ Property=""}
							}
						},
						Filters= new List<ConnectorsFiltersModel>()


					}
			};
		}

		#endregion

		[TestMethod]
		public async Task CallConnectorData()
		{
			// ARRANGE
			_mockConnectorDataService.Setup(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>())).Returns(Task.FromResult<dynamic>(JsonConvert.DeserializeObject(TwoDocumentsListJson)));

			ConnectorsController connector = CreateConnectorsController();

			// ACT
			var response = await connector.ConnectorData("fakeId", "fakeHost", null,true);

			// ASSERT
			_mockConnectorDataService.Verify(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()), Times.Exactly(1));
			var result = (ContentResult)response.Result;
			Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
		}

		[TestMethod]
		public async Task CallConnectorDataAndGetNullResponse()
		{
			// ARRANGE
			_mockConnectorDataService.Setup(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>())).Returns(Task.FromResult<dynamic>(null));

			ConnectorsController connector = CreateConnectorsController();

            // ACT
            var response = await connector.ConnectorData("fakeId", "fakeHost", null, true);

			// ASSERT
			_mockConnectorDataService.Verify(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()), Times.Exactly(1));
			var result = response.Value;
			Assert.AreEqual("Connector or data not found", result.Message);
		}

		[TestMethod]
		public async Task CallConnectors()
		{
			// ARRANGE
			var connections = MockConnectorData.SetupConnectorModel();
            Zurich.Common.Models.Connectors.ConnectorFilterModel filters = new Zurich.Common.Models.Connectors.ConnectorFilterModel();
			_mockConnectorservice.Setup(x => x.GetConnectors(It.IsAny<Zurich.Common.Models.Connectors.ConnectorFilterModel>())).Returns(Task.FromResult(connections.ToList()));

            ConnectorsController connector = new ConnectorsController(_mockConnectorservice.Object, _mockConnectorDataService.Object, null, _mapper,null);

			// ACT
			var response = await connector.Connectors(filters);

			// ASSERT
			_mockConnectorservice.Verify(x => x.GetConnectors(It.IsAny<Zurich.Common.Models.Connectors.ConnectorFilterModel>()), Times.Exactly(1));
			var result = (ContentResult)response.Result;
			Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
		}

		[TestMethod]
		public async Task CallConnectorsById()
		{
			// ARRANGE
			var connections = MockConnectorData.SetupConnectorModel().ToList()[0];
			_mockConnectorservice.Setup(x => x.GetConnector(It.IsAny<string>())).Returns(Task.FromResult(connections));

			ConnectorsController connector = CreateConnectorsController();

			// ACT
			var response = await connector.Connectors("1");

			// ASSERT
			_mockConnectorservice.Verify(x => x.GetConnector(It.IsAny<string>()), Times.Exactly(1));
			var result = response.Result;
			Assert.AreEqual(StatusCodes.Status200OK, ((Microsoft.AspNetCore.Mvc.ObjectResult)result).StatusCode);
		}

		[TestMethod]
		public async Task Call_Connector_Data_And_Get_Null_Response()
		{
			// ARRANGE
			_mockConnectorservice.Setup(x => x.GetConnector(It.IsAny<string>()));

			ConnectorsController connector = CreateConnectorsController();

			// ACT
			async Task result() => await connector.Connectors("22");

			// ASSERT
			await Assert.ThrowsExceptionAsync<ResourceNotFoundException>(result);
		}

		[TestMethod]
		public async Task CallConnectorDatawithStaticFilters()
		{
			// ARRANGE
			_mockConnectorDataService.Setup(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>())).Returns(Task.FromResult<dynamic>(JsonConvert.DeserializeObject(StaticFilterJson)));

			ConnectorsController connector = CreateConnectorsController();

			// ACT
			var response = await connector.ConnectorData("fakeId", "fakeHost", null, true);

			// ASSERT
			_mockConnectorDataService.Verify(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()), Times.Exactly(1));
			var result = (ContentResult)response.Result;
			Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
		}

		private ConnectorsController CreateConnectorsController()
		{
			var httpContext = new DefaultHttpContext();
			httpContext.Request.Headers["Authorization"] = "FakeToken";
			httpContext.Request.Query = null;

			var controllerContext = new ControllerContext()
			{
				HttpContext = httpContext,
			};

			return new ConnectorsController(_mockConnectorservice.Object, _mockConnectorDataService.Object, null, _mockmapper.Object,null) { ControllerContext = controllerContext };
		}
	}
}
