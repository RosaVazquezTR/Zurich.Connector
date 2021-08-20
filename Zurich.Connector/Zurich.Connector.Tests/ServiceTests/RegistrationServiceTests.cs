using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using Zurich.Connector.App.Services;
using Zurich.TenantData;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using System.Collections.Generic;
using Zurich.Connector.App.Model;
using System.Linq.Expressions;
using System.Linq;

namespace Zurich.Connector.Tests.ServiceTests
{
    [TestClass]
    public class RegistrationServiceTests
    {
        private Mock<ISessionAccessor> _mockSessionAccessor;
        private Mock<ICosmosService> _mockCosmosService;

        #region Data Model
        List<ConnectorRegistration> registrations = new List<ConnectorRegistration>()
        {
            {
                new ConnectorRegistration()
                {
                    ConnectorId = "1",
                    UserId = "userId1",
                    TenantId = "tenantid1",
                    AppName = "testApp"
                }
            },
            {
                new ConnectorRegistration()
                {
                    ConnectorId = "2",
                    UserId = "userId2",
                    TenantId = "tenantid2",
                    AppName = "testApp2"
                }
            },
            {
                new ConnectorRegistration()
                {
                    ConnectorId = "3",
                    UserId = "userId3",
                    TenantId = "tenantid3",
                    AppName = "testApp3"
                }
            }
        };

        #endregion

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
            var testId = "";
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

        [TestMethod]
        public async Task CallGetUserConnections()
        {
            var testIds = new List<string>() { "1", "2", "3" };
            //Arrange
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockCosmosService.Setup(x => x.GetConnectorRegistrations(It.IsAny<string>(), It.IsAny<Expression<Func<ConnectorRegistrationDocument, bool>>>())).Returns(registrations);

            var registrationService = new RegistrationService(_mockCosmosService.Object, _mockSessionAccessor.Object);

            //Act
            var registerIds = registrationService.GetUserConnections(null).ToList();

            //Assert
            _mockCosmosService.Verify(x => x.GetConnectorRegistrations(It.IsAny<string>(), It.IsAny<Expression<Func<ConnectorRegistrationDocument, bool>>>()), times: Times.Once());
            CollectionAssert.AreEqual(testIds, registerIds);
        }
    }
    
   
}
