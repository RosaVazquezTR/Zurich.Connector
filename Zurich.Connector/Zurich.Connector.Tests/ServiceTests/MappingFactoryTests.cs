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
			
			Type inputType = null;
			_serviceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
				.Callback<Type>(serviceType => inputType = serviceType);


			DataMappingFactory factory = new DataMappingFactory(_serviceProvider.Object, null);

			// ACT
			var response = factory.GetMapper(AuthType.TransferToken);

			// ASSERT
			_serviceProvider.Verify(x => x.GetService(It.IsAny<Type>()), Times.Once());
			Assert.AreEqual(typeof(DataMappingTransfer), inputType);
		}

		[TestMethod]
		public async Task GetDataMapperWithOAuth()
		{
			
			Type inputType = null;
			_serviceProvider.Setup(x => x.GetService(It.IsAny<Type>()))
				.Callback<Type>(serviceType => inputType = serviceType);


			DataMappingFactory factory = new DataMappingFactory(_serviceProvider.Object, null);

			// ACT
			var response = factory.GetMapper(AuthType.OAuth2);

			// ASSERT
			_serviceProvider.Verify(x => x.GetService(It.IsAny<Type>()), Times.Once());
			Assert.AreEqual(typeof(DataMappingOAuth), inputType);
		}

	}
}
