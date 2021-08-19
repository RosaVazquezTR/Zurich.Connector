using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Zurich.Connector.App.Services;
using Zurich.TenantData;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Tests.ServiceTests
{
    [TestClass]
    public class RegistrationServiceTests
    {
        private Mock<ISessionAccessor> _mockSessionAccessor;
        private Mock<ICosmosService> _mockCosmosService;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockSessionAccessor = new Mock<ISessionAccessor>();
            _mockCosmosService = new Mock<ICosmosService>();
        }

        [TestMethod]
        public async Task RegisterInvalidConnector()
        {
            //Arrange
            var testId = "140";
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            var tenantId = new Guid("d564ff78-bdab-4bf4-c9ae-08d83232798c");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockSessionAccessor.Setup(x => x.TenantId).Returns(tenantId);

            var registrationService = new RegistrationService(_mockCosmosService.Object, _mockSessionAccessor.Object);
            //Act
            bool register = await registrationService.RegisterDataSource(testId);
            //Assert
            _mockCosmosService.Verify(x => x.StoreConnectorRegistration(It.IsAny<ConnectorRegistrationDocument>()), times: Times.Never());
            Assert.IsFalse(register);
        }

        [TestMethod]
        public async Task RegisterValidConnector()
        {
            //Arrange
            var testId = "140";
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            var tenantId = new Guid("d564ff78-bdab-4bf4-c9ae-08d83232798c");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockSessionAccessor.Setup(x => x.TenantId).Returns(tenantId);

            var registrationService = new RegistrationService(_mockCosmosService.Object, _mockSessionAccessor.Object);
            //Act
            bool register = await registrationService.RegisterDataSource(testId);
            //Assert
            _mockCosmosService.Verify(x => x.StoreConnectorRegistration(It.IsAny<ConnectorRegistrationDocument>()), times: Times.Once());
            Assert.IsTrue(register);
        }
    }
    
   
}
