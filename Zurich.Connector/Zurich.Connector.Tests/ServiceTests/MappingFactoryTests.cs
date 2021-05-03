using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Services;

namespace Zurich.Connector.Tests.ServiceTests
{
	[TestClass]
	public class MappingFactoryTests
	{
		private Mock<IServiceProvider> _serviceProvider;

		[TestInitialize]
		public void TestInitialize()
		{
			_serviceProvider = new Mock<IServiceProvider>();
		}

		[TestMethod]
		public async Task GetDataMapperWithTransferAuth()
		{
			// ARRANGE
			DataMappingClass dataMap = new DataMappingClass()
			{
				Api = new DataMappingApiRequest() { Url = "https://fakeaddress.thomsonreuters.com", AuthHeader = "differentAuthHeader" },
				AuthType = AuthType.TransferToken,
				AppCode = "fakeCode",
				ResultLocation = "data.results",
				Mapping = new List<DataMappingProperty>() {
					new DataMappingProperty(){CDMProperty = "Name", APIProperty =  "name"},
				}
			};

			Type inputType = null;
			_serviceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
				.Callback<Type>(serviceType => inputType = serviceType);


			DataMappingFactory factory = new DataMappingFactory(_serviceProvider.Object, null);

			// ACT
			var response = factory.GetMapper(dataMap);

			// ASSERT
			_serviceProvider.Verify(x => x.GetService(It.IsAny<Type>()), Times.Once());
			Assert.AreEqual(typeof(DataMappingTransfer), inputType);
		}

		[TestMethod]
		public async Task GetDataMapperWithOAuth()
		{
			// ARRANGE
			DataMappingClass dataMap = new DataMappingClass()
			{
				Api = new DataMappingApiRequest() { Url = "https://fakeaddress.thomsonreuters.com", AuthHeader = "differentAuthHeader" },
				AuthType = AuthType.OAuth,
				AppCode = "fakeCode",
				ResultLocation = "data.results",
				Mapping = new List<DataMappingProperty>() {
					new DataMappingProperty(){CDMProperty = "Name", APIProperty =  "name"},
				}
			};

			Type inputType = null;
			_serviceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
				.Callback<Type>(serviceType => inputType = serviceType);


			DataMappingFactory factory = new DataMappingFactory(_serviceProvider.Object, null);

			// ACT
			var response = factory.GetMapper(dataMap);

			// ASSERT
			_serviceProvider.Verify(x => x.GetService(It.IsAny<Type>()), Times.Once());
			Assert.AreEqual(typeof(DataMappingOAuth), inputType);
		}

	}
}
