using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Web.Controllers;

namespace Zurich.Connector.Tests.ControllerTests
{
	[TestClass]
	public class ConnectorTests
	{
		private Mock<IConnectorService> _mockConnector;

		[TestInitialize]
		public void TestInitialize()
		{
			_mockConnector = new Mock<IConnectorService>();
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


		#endregion

		[TestMethod]
		public async Task CallConnectorData()
		{
			// ARRANGE
			_mockConnector.Setup(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<dynamic>(TwoDocumentsListJson));

			ConnectorsController connector = new ConnectorsController(_mockConnector.Object, null);
			
			// ACT
			var response = await connector.ConnectorData("fakeId", "fakeHost", null);

			// ASSERT
			_mockConnector.Verify(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(1));
			var result = (ContentResult)response.Result;
			Assert.AreEqual(StatusCodes.Status200OK, result.StatusCode);
		}

		[TestMethod]
		public async Task CallConnectorDataAndGetNullResponse()
		{
			// ARRANGE
			_mockConnector.Setup(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.FromResult<dynamic>(null));

			ConnectorsController connector = new ConnectorsController(_mockConnector.Object, null);

			// ACT
			var response = await connector.ConnectorData("fakeId", "fakeHost", null);

			// ASSERT
			_mockConnector.Verify(x => x.GetConnectorData(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(1));
			var result = (NotFoundObjectResult)response.Result;
			Assert.AreEqual(StatusCodes.Status404NotFound, result.StatusCode);
		}

	}
}
