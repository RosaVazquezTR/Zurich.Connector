using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Web;
using Zurich.Connector.Web.Controllers;

namespace Zurich.Connector.Tests.ControllerTests
{
	[TestClass]
	public class ConnectorTests
	{
		private Mock<IConnectorService> _mockConnectorservice;
		private IMapper _mapper;
		private Mock<IMapper> _mockmapper;

		[TestInitialize]
		public void TestInitialize()
		{
			_mockConnectorservice = new Mock<IConnectorService>();

			var mapConfig = new MapperConfiguration(cfg =>
			{
				cfg.AddProfile(new MappingRegistrar());
			});
			_mapper = mapConfig.CreateMapper();
			_mockmapper = new Mock<IMapper>();
		}

		#region json Strings

		private const string TwoDocumentsListJson = @"
		[{
				""author_description"": ""Ryan Hunecke"",
				""author"": ""RYAN.HUNECKE"",
				""class"": ""DOC"",
				""class_description"": ""Document""
			}, {
				""author_description"": ""Sally Sales"",
				""author"": ""ALEX.PRICE""
			}
		]";

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
		public async Task CallConnectorData()
		{
			// ARRANGE
			_mockConnectorservice.Setup(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),It.IsAny<Dictionary<string, string>>())).Returns(Task.FromResult<dynamic>(TwoDocumentsListJson));

			ConnectorsController connector = CreateConnectorsController();
			
			// ACT
			var response = await connector.ConnectorData("fakeId", "fakeHost", null);

			// ASSERT
			_mockConnectorservice.Verify(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Exactly(1));
			var result = (ContentResult)response.Result;
			Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
		}

		[TestMethod]
		public async Task CallConnectorDataAndGetNullResponse()
		{
			// ARRANGE
			_mockConnectorservice.Setup(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>())).Returns(Task.FromResult<dynamic>(null));

			ConnectorsController connector = CreateConnectorsController();

			// ACT
			var response = await connector.ConnectorData("fakeId", "fakeHost", null);

			// ASSERT
			_mockConnectorservice.Verify(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()), Times.Exactly(1));
			var result = (NotFoundObjectResult)response.Result;
			Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
		}

		[TestMethod]
		public async Task CallConnectors()
		{
			// ARRANGE
			var connections = SetupConnections();
			ConnectorFilterModel filters = new ConnectorFilterModel();
			_mockConnectorservice.Setup(x => x.GetConnectors(It.IsAny<ConnectorFilterModel>())).Returns(Task.FromResult(connections));

			ConnectorsController connector = new ConnectorsController(_mockConnectorservice.Object, null, _mapper);

			// ACT
			var response = await connector.Connectors(filters);

			// ASSERT
			_mockConnectorservice.Verify(x => x.GetConnectors(It.IsAny<ConnectorFilterModel>()), Times.Exactly(1));
			var result = (ContentResult)response.Result;
			Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
		}

		[TestMethod]
		public async Task CallSearchConnectorData()
		{
			// ARRANGE
			_mockConnectorservice.Setup(x => x.GetConnectorConfiguration(It.IsAny<string>())).Returns(Task.FromResult(Connector_Configuration));

			ConnectorsController connector = CreateConnectorsController();

			// ACT
			var response = await connector.ConnectorconfigData("fakeId");

			// ASSERT
			_mockConnectorservice.Verify(x => x.GetConnectorConfiguration(It.IsAny<string>()), Times.Exactly(1));
			var result = response.Result;
		    Assert.AreEqual(StatusCodes.Status200OK, ((Microsoft.AspNetCore.Mvc.ObjectResult)result).StatusCode);
		}

		[TestMethod]
		public async Task Call_Connector_Data_And_Get_Null_Response()
		{
			// ARRANGE
			_mockConnectorservice.Setup(x => x.GetConnectorConfiguration(It.IsAny<string>()));

			ConnectorsController connector = CreateConnectorsController();

			// ACT
			var response = await connector.ConnectorconfigData("fakeId");

			// ASSERT
			_mockConnectorservice.Verify(x => x.GetConnectorConfiguration(It.IsAny<string>()), Times.Exactly(1));
			var result = (NotFoundObjectResult)response.Result;
			Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
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

			return new ConnectorsController(_mockConnectorservice.Object, null, _mockmapper.Object) { ControllerContext = controllerContext };
		}
	}
}
