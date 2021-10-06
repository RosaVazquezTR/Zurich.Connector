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
using Zurich.Common.Models.Connectors;

namespace Zurich.Connector.Tests.ServiceTests
{
    [TestClass]
    public class RegistrationServiceTests
    {
        private Mock<ISessionAccessor> _mockSessionAccessor;
        private Mock<ICosmosService> _mockCosmosService;
        private Mock<IOAuthServices> _mockOAuthService;

        #region Data Model
        List<ConnectorRegistration> registrations = new List<ConnectorRegistration>()
        {
            {
                new ConnectorRegistration()
                {
                    Id = "abc-1",
                    ConnectorId = "1",
                    UserId = "userId1",
                    TenantId = "tenantid1",
                    AppName = "testApp"
                }
            },
            {
                new ConnectorRegistration()
                {
                    Id = "abc-2",
                    ConnectorId = "2",
                    UserId = "userId2",
                    TenantId = "tenantid2",
                    AppName = "testApp2"
                }
            },
            {
                new ConnectorRegistration()
                {
                    Id = "abc-3",
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
            _mockOAuthService = new Mock<IOAuthServices>();
        }


        [TestMethod]
        public async Task RegisterInvalidConnector()
        {
            //Arrange
            var testId = "";
            var appCode = "";
            var registraionMode = RegistrationEntityMode.Automatic;
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            var tenantId = new Guid("d564ff78-bdab-4bf4-c9ae-08d83232798c");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockSessionAccessor.Setup(x => x.TenantId).Returns(tenantId);

            var registrationService = new RegistrationService(_mockCosmosService.Object, _mockSessionAccessor.Object, _mockOAuthService.Object);
            //Act
            bool register = await registrationService.RegisterDataSource(testId, appCode, registraionMode);
            //Assert
            _mockCosmosService.Verify(x => x.StoreConnectorRegistration(It.IsAny<ConnectorRegistrationDocument>()), times: Times.Never());
            Assert.IsFalse(register);
        }

        [TestMethod]
        public async Task RegisterValidConnector()
        {
            //Arrange
            var testId = "140";
            var appCode = "";
            var registraionMode = RegistrationEntityMode.Manual;
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            var tenantId = new Guid("d564ff78-bdab-4bf4-c9ae-08d83232798c");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockSessionAccessor.Setup(x => x.TenantId).Returns(tenantId);

            var registrationService = new RegistrationService(_mockCosmosService.Object, _mockSessionAccessor.Object, _mockOAuthService.Object);
            //Act
            bool register = await registrationService.RegisterDataSource(testId, appCode, registraionMode);
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

            var registrationService = new RegistrationService(_mockCosmosService.Object, _mockSessionAccessor.Object, _mockOAuthService.Object);

            //Act
            var registerIds = registrationService.GetUserConnections(null).ToList();

            //Assert
            _mockCosmosService.Verify(x => x.GetConnectorRegistrations(It.IsAny<string>(), It.IsAny<Expression<Func<ConnectorRegistrationDocument, bool>>>()), times: Times.Once());
            CollectionAssert.AreEqual(testIds, registerIds);
        }

        [TestMethod]
        public async Task CallRemoveUserConnectorWithValidRegistration()
        {
            //Arrange
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockCosmosService.Setup(x => x.GetConnectorRegistrations(It.IsAny<string>(), It.IsAny<Expression<Func<ConnectorRegistrationDocument, bool>>>())).Returns(registrations);
            _mockCosmosService.Setup(x => x.RemoveConnectorRegistration(It.IsAny<string>(), It.IsAny<string>()));

            var registrationService = new RegistrationService(_mockCosmosService.Object, _mockSessionAccessor.Object, _mockOAuthService.Object);

            //Act
            var success = await registrationService.RemoveUserConnector("1");

            //Assert
            _mockCosmosService.Verify(x => x.RemoveConnectorRegistration(It.IsAny<string>(), It.IsAny<string>()), times: Times.Exactly(3));
            Assert.AreEqual(true, success);
        }

        [TestMethod]
        public async Task CallRemoveUserConnectorWithInvalidRegistration()
        {
            //Arrange
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockCosmosService.Setup(x => x.GetConnectorRegistrations(It.IsAny<string>(), It.IsAny<Expression<Func<ConnectorRegistrationDocument, bool>>>())).Returns(new List<ConnectorRegistration>());
            _mockCosmosService.Setup(x => x.RemoveConnectorRegistration(It.IsAny<string>(), It.IsAny<string>()));

            var registrationService = new RegistrationService(_mockCosmosService.Object, _mockSessionAccessor.Object, _mockOAuthService.Object);

            //Act
            var success = await registrationService.RemoveUserConnector("1");

            //Assert
            _mockCosmosService.Verify(x => x.RemoveConnectorRegistration(It.IsAny<string>(), It.IsAny<string>()), times: Times.Never());
            Assert.AreEqual(false, success);
        }

        [TestMethod]
        public async Task RegisterValidConnectorwithAutomaticregistration()
        {
            //Arrange
            var testId = "140";
            var appCode = "PLCUS";
            var registraionMode = RegistrationEntityMode.Automatic;
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            var tenantId = new Guid("d564ff78-bdab-4bf4-c9ae-08d83232798c");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockSessionAccessor.Setup(x => x.TenantId).Returns(tenantId);
            _mockOAuthService.Setup(x => x.AutomaticRegistration(appCode)).Returns(Task.FromResult<bool>(true));

            var registrationService = new RegistrationService(_mockCosmosService.Object, _mockSessionAccessor.Object, _mockOAuthService.Object);
            //Act
            bool register = await registrationService.RegisterDataSource(testId, appCode, registraionMode);
            //Assert
            _mockCosmosService.Verify(x => x.StoreConnectorRegistration(It.IsAny<ConnectorRegistrationDocument>()), times: Times.Once());
            _mockOAuthService.Verify(x => x.AutomaticRegistration(appCode), times: Times.Once());
            Assert.IsTrue(register);
        }
        [TestMethod]
        public async Task AutoRegister_validConnector_With_InvalidAppcode()
        {
            //Arrange
            var testId = "14";
            var appCode = "PLCUK";
            var registraionMode = RegistrationEntityMode.Automatic;
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            var tenantId = new Guid("d564ff78-bdab-4bf4-c9ae-08d83232798c");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockSessionAccessor.Setup(x => x.TenantId).Returns(tenantId);
            _mockOAuthService.Setup(x => x.AutomaticRegistration(appCode)).Returns(Task.FromResult<bool>(false));

            var registrationService = new RegistrationService(_mockCosmosService.Object, _mockSessionAccessor.Object, _mockOAuthService.Object);
            //Act
            bool register = await registrationService.RegisterDataSource(testId, appCode, registraionMode);
            //Assert
            _mockCosmosService.Verify(x => x.StoreConnectorRegistration(It.IsAny<ConnectorRegistrationDocument>()), times: Times.Once());
            _mockOAuthService.Verify(x => x.AutomaticRegistration(It.IsAny<String>()), times: Times.Once());
            Assert.IsFalse(register);
        }
    }


}
