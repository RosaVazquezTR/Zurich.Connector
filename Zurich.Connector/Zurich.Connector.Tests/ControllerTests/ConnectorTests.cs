using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
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
using Zurich.Connector.Web.Models;

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

		private const string TwoDocumentsListArrayJson = @"
		[
			{
				""author_description"": ""Ryan Hunecke"",
				""author"": ""RYAN.HUNECKE"",
				""class"": ""DOC"",
				""class_description"": ""Document""
			}, {
				""author_description"": ""Sally Sales"",
				""author"": ""ALEX.PRICE""
			}
		]";

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

		private const string StaticFilterJsonNotMultiselect = @"
		{
			""results"":[{
				""name"": ""Available Filters"",
				""description"": ""WLUK available filters"",
				""isMultiselect"": ""false"",
				""requestParameter"": ""Content.default"",
				""filterlist"": [
					{
						""name"": ""Finance"",
						""id"": ""Home/WestlawUK/Topic/Finance""

					},
					{
						""name"": ""Cases"",
						""id"": ""Home/WestlawUK/Cases""

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

		private ConnectorModel mockConnectorModel = new ConnectorModel()
		{
			Filters = new List<ConnectorsFiltersModel>()
			{
				new ConnectorsFiltersModel()
				{
					Name = "Topics",
					Description = "WLUK topics",
					IsMultiSelect = "false",
					RequestParameter = "Content.default",
					FilterList = new List<FilterListModel>()
					{
						new FilterListModel()
						{
							Name = "Cases",
							Id = "Home/WestlawUK/Cases"
						},
						new FilterListModel()
						{
							Name = "Tax",
							Id = "Home/WestlawUK/Topic/Tax"
						}
					}
				}
			},
			Info = new ConnectorInfoModel()
            {
				AcceptsSearchWildCard = true
            }
		};

		#endregion

		[TestMethod]
		public async Task CallConnectorDataReturnObject()
		{
			// ARRANGE
			_mockConnectorDataService.Setup(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>())).Returns(Task.FromResult<dynamic>(JsonConvert.DeserializeObject(TwoDocumentsListJson)));
			_mockConnectorservice.Setup(x => x.GetConnector(It.IsAny<string>())).Returns(Task.FromResult<ConnectorModel>(mockConnectorModel));

			ConnectorsController connector = CreateConnectorsController();

			// ACT
			var response = await connector.ConnectorData("fakeId", "fakeHost", null,true);

			// ASSERT
			_mockConnectorDataService.Verify(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()), Times.Exactly(1));
			var result = (ContentResult)response.Result;
			Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
		}

		[TestMethod]
		public async Task CallConnectorDataReturnArray()
		{
			// ARRANGE
			_mockConnectorDataService.Setup(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>())).Returns(Task.FromResult<dynamic>(JsonConvert.DeserializeObject(TwoDocumentsListArrayJson)));
			_mockConnectorservice.Setup(x => x.GetConnector(It.IsAny<string>())).Returns(Task.FromResult<ConnectorModel>(mockConnectorModel));

			ConnectorsController connector = CreateConnectorsController();

			// ACT
			var response = await connector.ConnectorData("fakeId", "fakeHost", null, true);

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
			_mockConnectorservice.Setup(x => x.GetConnector(It.IsAny<string>())).Returns(Task.FromResult<ConnectorModel>(mockConnectorModel));

			ConnectorsController connector = CreateConnectorsController();

            // ACT
            var response = await connector.ConnectorData("fakeId", "fakeHost", null, true);

			// ASSERT
			_mockConnectorDataService.Verify(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()), Times.Exactly(1));
			var result = (ContentResult)response.Result;
			Assert.AreEqual("Connector or data not found", result.Content);
		}

		[TestMethod]
		public async Task CallConnectors()
		{
			// ARRANGE
			var connections = MockConnectorData.SetupConnectorModel();
            ConnectorFilterViewModel filters = new ConnectorFilterViewModel();
			_mockConnectorservice.Setup(x => x.GetConnectors(It.IsAny<ConnectorFilterModel>())).Returns(Task.FromResult(connections));

            ConnectorsController connector = new ConnectorsController(_mockConnectorservice.Object, _mockConnectorDataService.Object, _mapper,null);

			// ACT
			var response = await connector.Connectors(filters);

			// ASSERT
			_mockConnectorservice.Verify(x => x.GetConnectors(It.IsAny<ConnectorFilterModel>()), Times.Exactly(1));
			var result = (ContentResult)response.Result;
			Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
		}

		[TestMethod]
		public async Task CallConnectorsById()
		{
			// ARRANGE
			var connections = MockConnectorData.SetupConnectorModel().ToList()[3];
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
			_mockConnectorservice.Setup(x => x.GetConnector(It.IsAny<string>())).Returns(Task.FromResult<ConnectorModel>(mockConnectorModel));

			ConnectorsController connector = CreateConnectorsController();

			// ACT
			var response = await connector.ConnectorData("fakeId", "fakeHost", null, true);

			// ASSERT
			_mockConnectorDataService.Verify(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()), Times.Exactly(1));
			var result = (ContentResult)response.Result;
			Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
		}

		/// <summary>
		/// Test to ensure that when sending multiple values for a filter that doesn't support it, it returns a bad request
		/// </summary>
		[TestMethod]
		public async Task CallConnectorDataWithWrongStaticFilters()
		{
			// ARRANGE
			_mockConnectorDataService.Setup(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>())).Returns(Task.FromResult<dynamic>(JsonConvert.DeserializeObject(StaticFilterJsonNotMultiselect)));
			_mockConnectorservice.Setup(x => x.GetConnector(It.IsAny<string>())).Returns(Task.FromResult<ConnectorModel>(mockConnectorModel));

			ConnectorsController connector = CreateConnectorsControllerWithQuery();

			// ACT
			var response = await connector.ConnectorData("fakeId", "fakeHost", null, true);

			// ASSERT
			//_mockConnectorDataService.Verify(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<bool>()), Times.Exactly(1));
			var result = (ContentResult)response.Result;
			Assert.AreEqual(StatusCodes.Status400BadRequest, result.StatusCode);
		}

		[TestMethod]
		public async Task CalliManageConnectordetails()
		{
			// ARRANGE
			var connections = MockConnectorData.SetupConnectorModel_Version2().ToList()[0];
			_mockConnectorservice.Setup(x => x.GetConnector(It.IsAny<string>())).Returns(Task.FromResult(connections));

			ConnectorsController connector = new ConnectorsController(_mockConnectorservice.Object, _mockConnectorDataService.Object, _mapper, null);

			// ACT
			var response = await connector.Connectors("44");

			// ASSERT
			_mockConnectorservice.Verify(x => x.GetConnector(It.IsAny<string>()), Times.Exactly(1));
			var result = response.Result;
			Assert.AreEqual(StatusCodes.Status200OK, ((Microsoft.AspNetCore.Mvc.ObjectResult)result).StatusCode);
		}

		[TestMethod]
		public async Task TestiManageConnectordetailscount()
		{
			// ARRANGE
			var connections = MockConnectorData.SetupConnectorModel_Version2().ToList()[3];
			_mockConnectorservice.Setup(x => x.GetConnector(It.IsAny<string>())).Returns(Task.FromResult(connections));

			ConnectorsController connector = new ConnectorsController(_mockConnectorservice.Object, _mockConnectorDataService.Object, _mapper, null);

			// ACT
			dynamic response = await connector.Connectors("44");

			// ASSERT
			_mockConnectorservice.Verify(x => x.GetConnector(It.IsAny<string>()), Times.Exactly(1));
			var result = response.Result.Value;
			Assert.AreEqual(connections.CDMMapping.Unstructured.Count, result.CDMMapping.Unstructured.Count);
		}

		[TestMethod]
		public async Task TestiManageDomainValue()
		{
			// ARRANGE
			var connections = MockConnectorData.SetupConnectorModel_Version2().ToList()[3];
			_mockConnectorservice.Setup(x => x.GetConnector(It.IsAny<string>())).Returns(Task.FromResult(connections));

			ConnectorsController connector = new ConnectorsController(_mockConnectorservice.Object, _mockConnectorDataService.Object, _mapper, null);

			// ACT
			dynamic response = await connector.Connectors("44");

			// ASSERT
			_mockConnectorservice.Verify(x => x.GetConnector(It.IsAny<string>()), Times.Exactly(1));
			Assert.AreEqual(connections.DataSource.Domain, "TestDomain.com");
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

			return new ConnectorsController(_mockConnectorservice.Object, _mockConnectorDataService.Object, _mockmapper.Object,null) { ControllerContext = controllerContext };
		}

		private ConnectorsController CreateConnectorsControllerWithQuery()
		{
			var httpContext = new DefaultHttpContext();
			httpContext.Request.Headers["Authorization"] = "FakeToken";
            httpContext.Request.Query = new QueryCollection(new Dictionary<string, StringValues>()
            {
                { "retrieveFilters", "True" },
                { "Query", "economic substance" },
                { "Offset", "0" },
                { "ResultSize", "25" },
                { "Content.default", "Home/WestlawUK/WLLegislation/PolicyAndGuidance,Home/WestlawUK/Cases" }
            });

            var controllerContext = new ControllerContext()
			{
				HttpContext = httpContext,
			};

			return new ConnectorsController(_mockConnectorservice.Object, _mockConnectorDataService.Object, _mockmapper.Object, null) { ControllerContext = controllerContext };
		}
	}
}
