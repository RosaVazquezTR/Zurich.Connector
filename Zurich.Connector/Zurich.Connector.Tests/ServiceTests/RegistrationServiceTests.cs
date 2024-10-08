﻿using Microsoft.ApplicationInsights;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Zurich.Common.Models.Connectors;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Repositories;
using Zurich.Common.Services;
using Zurich.Connector.App;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.TenantData;
using Zurich.TenantData.Models;
using AppConnectorModel = Zurich.Connector.App.Model.ConnectorModel;
using DataModel = Zurich.Connector.Data.Model;
using DataModels = Zurich.Connector.Data.Model;
using DataSourceModel = Zurich.Connector.App.Model.DataSourceModel;

namespace Zurich.Connector.Tests.ServiceTests
{
    [TestClass]
    public class RegistrationServiceTests
    {
        private Mock<ISessionAccessor> _mockSessionAccessor;
        private Mock<ICosmosService> _mockCosmosService;
        private Mock<IOAuthServices> _mockOAuthService;
        private Mock<IOAuthApiRepository> _mockOAuthApiRepository;
        private Mock<IConfiguration> _mockConfiguration;
        private Mock<ITenantService> _mockTenantService;
        private Mock<ILegalHomeAccessCheck> _mockLegalHomeAccess;
        private Mock<OAuthOptions> _mockOAuthOptions;
        private TelemetryClient _telemetryClientMock;
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
            _mockOAuthApiRepository = new Mock<IOAuthApiRepository>();
            _mockConfiguration = new Mock<IConfiguration>();
            _mockTenantService = new Mock<ITenantService>();
            _mockLegalHomeAccess = new Mock<ILegalHomeAccessCheck>();
            _mockOAuthOptions = new Mock<OAuthOptions>();
            _telemetryClientMock = new TelemetryClient();
        }

        private RegistrationService CreateService(IConfiguration config = null)
        {
            if (config == null)
                config = _mockConfiguration.Object;
            return new RegistrationService(_mockCosmosService.Object, _mockSessionAccessor.Object, _mockOAuthService.Object, _mockOAuthApiRepository.Object, config, _mockTenantService.Object, _mockLegalHomeAccess.Object, _mockOAuthOptions.Object, _telemetryClientMock);
        }

        [TestMethod]
        public async Task RegisterInvalidConnector()
        {
            //Arrange
            var testId = "";
            var appCode = "";
            var registrationMode = RegistrationEntityMode.Automatic;
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            var tenantId = new Guid("d564ff78-bdab-4bf4-c9ae-08d83232798c");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockSessionAccessor.Setup(x => x.TenantId).Returns(tenantId);

            var registrationService = CreateService();
            //Act
            DataModels.DataSourceRegistration register = await registrationService.RegisterConnector(testId);
            //Assert
            _mockCosmosService.Verify(x => x.StoreConnectorRegistration(It.IsAny<ConnectorRegistrationDocument>()), times: Times.Never());
            Assert.IsNull(register);
        }

        [TestMethod]
        public async Task RegisterValidConnector()
        {
            //Arrange
            var testId = "140";
            var appCode = "PLCUS";
            //var registrationMode = RegistrationEntityMode.Manual;
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            var tenantId = new Guid("d564ff78-bdab-4bf4-c9ae-08d83232798c");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockSessionAccessor.Setup(x => x.TenantId).Returns(tenantId);

            AppConnectorModel connectors = new AppConnectorModel() { DataSource = new DataSourceModel() { AppCode = appCode, Domain = "" } } ;
            _mockCosmosService.Setup(x => x.GetConnectorUsingPreRelease(It.IsAny<string>(), true)).Returns(Task.FromResult(connectors));

            List<DataModel.DataSourceInformation> userRegistrations = new List<DataModel.DataSourceInformation>();
            userRegistrations.Add(new DataModel.DataSourceInformation() { AppCode = appCode });
            _mockOAuthService
                .Setup(x => x.GetUserRegistrations())
                .Returns(Task.FromResult<IEnumerable<DataModel.DataSourceInformation>>(userRegistrations.AsEnumerable()));

            var registrationService = CreateService();

            //Act
            DataModels.DataSourceRegistration register = await registrationService.RegisterConnector(testId);
            //Assert
            Assert.IsNotNull(register);
            Assert.AreEqual(register.Registered, true);
        }

        [TestMethod]
        public async Task CallGetUserDataSources()
        {
            List<string> appcodes = new List<string>() { "iManage", "MSGraph", "fakeApp" };

            //Arrange
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            var cosmosApps = new List<App.Model.DataSourceModel>();
            cosmosApps.Add(new App.Model.DataSourceModel() { AppCode = "newApp", RegistrationInfo = new App.Model.RegistrationInfoModel() { RegistrationMode = RegistrationEntityMode.Automatic } });
            cosmosApps.Add(new App.Model.DataSourceModel() { AppCode = "NotReturned", RegistrationInfo = new App.Model.RegistrationInfoModel() { RegistrationMode = RegistrationEntityMode.Manual } });
            _mockCosmosService.Setup(x => x.GetDataSources(null)).Returns(Task.FromResult<IEnumerable<App.Model.DataSourceModel>>(cosmosApps));
            var apps = appcodes.Select(x=>new TenantMemberApplication() { ApplicationCode = x, CurrentToken_Id = new Guid() }).ToList();
            _mockTenantService.Setup(x => x.GetTenantMemberApps()).Returns(Task.FromResult(apps));
            _mockLegalHomeAccess.Setup(x => x.isLegalHomeUser()).Returns(true);

            var registrationService = CreateService();

            //Act
            var returnedDataSources = (await registrationService.GetUserDataSources()).ToList();

            //Assert
            Assert.IsNotNull(returnedDataSources);
            Assert.AreEqual(4, returnedDataSources.Count);
            Assert.IsFalse(returnedDataSources.Contains(new DataModel.DataSourceInformation() { AppCode = "NotReturned" }));
        }

        [TestMethod]
        public async Task CallGetUserDataSourcesNoToken()
        {
            List<string> appcodes = new List<string>() { "iManage", "MSGraph", "fakeApp" };

            //Arrange
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            var cosmosApps = new List<App.Model.DataSourceModel>();
            cosmosApps.Add(new App.Model.DataSourceModel() { AppCode = "newApp", RegistrationInfo = new App.Model.RegistrationInfoModel() { RegistrationMode = RegistrationEntityMode.Automatic } });
            cosmosApps.Add(new App.Model.DataSourceModel() { AppCode = "NotReturned", RegistrationInfo = new App.Model.RegistrationInfoModel() { RegistrationMode = RegistrationEntityMode.Manual } });
            _mockCosmosService.Setup(x => x.GetDataSources(null)).Returns(Task.FromResult<IEnumerable<App.Model.DataSourceModel>>(cosmosApps));
            var apps = appcodes.Select(x => new TenantMemberApplication() { ApplicationCode = x, CurrentToken_Id = new Guid() }).ToList();
            apps[2].CurrentToken_Id = null;
            _mockTenantService.Setup(x => x.GetTenantMemberApps()).Returns(Task.FromResult(apps));
            _mockLegalHomeAccess.Setup(x => x.isLegalHomeUser()).Returns(true);

            var registrationService = CreateService();

            //Act
            var returnedDataSources = (await registrationService.GetUserDataSources()).ToList();

            //Assert
            Assert.IsNotNull(returnedDataSources);
            Assert.AreEqual(3, returnedDataSources.Count);
            Assert.IsFalse(returnedDataSources.Contains(new DataModel.DataSourceInformation() { AppCode = "NotReturned" }));
            Assert.IsFalse(returnedDataSources.Contains(new DataModel.DataSourceInformation() { AppCode = "fakeApp" }));
        }

        [TestMethod]
        public async Task CallGetUserDataSourcesNullRegistrationInfo()
        {
            List<string> appcodes = new List<string>() { "iManage", "MSGraph", "fakeApp" };

            //Arrange
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            var cosmosApps = new List<App.Model.DataSourceModel>();
            cosmosApps.Add(new App.Model.DataSourceModel() { AppCode = "newApp", RegistrationInfo = null });
            cosmosApps.Add(new App.Model.DataSourceModel() { AppCode = "NotReturned", RegistrationInfo = new App.Model.RegistrationInfoModel() { RegistrationMode = RegistrationEntityMode.Manual } });
            _mockCosmosService.Setup(x => x.GetDataSources(null)).Returns(Task.FromResult<IEnumerable<App.Model.DataSourceModel>>(cosmosApps));
            var apps = appcodes.Select(x => new TenantMemberApplication() { ApplicationCode = x, CurrentToken_Id = new Guid() }).ToList();
            _mockTenantService.Setup(x => x.GetTenantMemberApps()).Returns(Task.FromResult(apps));
            _mockLegalHomeAccess.Setup(x => x.isLegalHomeUser()).Returns(true);

            var registrationService = CreateService();

            //Act
            var returnedDataSources = (await registrationService.GetUserDataSources()).ToList();

            //Assert
            Assert.IsNotNull(returnedDataSources);
            Assert.AreEqual(3, returnedDataSources.Count);
            Assert.IsFalse(returnedDataSources.Contains(new DataModel.DataSourceInformation() { AppCode = "NotReturned" }));
            Assert.IsFalse(returnedDataSources.Contains(new DataModel.DataSourceInformation() { AppCode = "newApp" }));
        }

        [TestMethod]
        public async Task CallRemoveUserConnectorWithValidRegistration()
        {
            //Arrange
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockCosmosService.Setup(x => x.GetConnectorRegistrations(It.IsAny<string>(), It.IsAny<Expression<Func<ConnectorRegistrationDocument, bool>>>())).Returns(registrations);
            _mockCosmosService.Setup(x => x.RemoveConnectorRegistration(It.IsAny<string>(), It.IsAny<string>()));

            var registrationService = CreateService();

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

            var registrationService = CreateService();

            //Act
            var success = await registrationService.RemoveUserConnector("1");

            //Assert
            _mockCosmosService.Verify(x => x.RemoveConnectorRegistration(It.IsAny<string>(), It.IsAny<string>()), times: Times.Never());
            Assert.AreEqual(false, success);
        }

        [TestMethod]
        public async Task RegisterValidConnectorwithAutomaticRegistration()
        {
            //Arrange
            var testId = "140";
            var appCode = "PLCUS";
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            var tenantId = new Guid("d564ff78-bdab-4bf4-c9ae-08d83232798c");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockSessionAccessor.Setup(x => x.TenantId).Returns(tenantId);
            _mockOAuthService.Setup(x => x.AutomaticRegistration(appCode)).Returns(Task.FromResult<bool>(true));

            AppConnectorModel connectors = new AppConnectorModel() 
                                                        { DataSource = new DataSourceModel() { AppCode = appCode, Domain = "Domain1", 
                                                          RegistrationInfo = new App.Model.RegistrationInfoModel()
                                                          { DomainRequired =true, RegistrationMode = RegistrationEntityMode.Automatic } } } ;
            _mockCosmosService.Setup(x => x.GetConnectorUsingPreRelease(It.IsAny<string>(), true)).Returns(Task.FromResult(connectors));


            List<DataModel.DataSourceInformation> userRegistrations = new List<DataModel.DataSourceInformation>();
            userRegistrations.Add(new DataModel.DataSourceInformation() { AppCode = "PLCUK" });
            _mockOAuthService
                .Setup(x => x.GetUserRegistrations())
                .Returns(Task.FromResult<IEnumerable<DataModel.DataSourceInformation>>(userRegistrations.AsEnumerable()));

            List<DataModel.DataSourceInformation> availableRegistrations = new List<DataModel.DataSourceInformation>();
            availableRegistrations.Add(new DataModel.DataSourceInformation() { AppCode = appCode, Domain = "Domain1" });
            _mockOAuthService.Setup(x => x.GetAvailableRegistrations()).Returns(Task.FromResult(availableRegistrations));

            var registrationService = CreateService();
            //Act
            DataModels.DataSourceRegistration register = await registrationService.RegisterConnector(testId);
            //Assert
            _mockOAuthService.Verify(x => x.AutomaticRegistration(appCode), times: Times.Once());
            Assert.IsNotNull(register);
            Assert.IsTrue(register.Registered);
        }

        [TestMethod]
        public async Task RegisterValidConnectorWithTenantWideRegistrationWithoutToken()
        {
            //Arrange
            var testId = "140";
            var appCode = "AppCode";
            var domain = "AppCodeDomain";
            var tenantId = Guid.NewGuid();
            _mockSessionAccessor.Setup(x => x.UserId).Returns(Guid.Empty);
            _mockSessionAccessor.Setup(x => x.TenantId).Returns(tenantId);
            _mockOAuthApiRepository.Setup(x => x.GetTokenWithTenantId(appCode, tenantId.ToString(), It.IsAny<string>())).ReturnsAsync(default(OAuthAPITokenResponse));
            _mockOAuthService.Setup(x => x.AutomaticRegistration(appCode)).ReturnsAsync(true);

            AppConnectorModel connectors = new AppConnectorModel()
            {
                DataSource = new DataSourceModel()
                {
                    AppCode = appCode,
                    Domain = domain,
                    RegistrationInfo = new App.Model.RegistrationInfoModel()
                    { DomainRequired = true, RegistrationMode = RegistrationEntityMode.TenantWide }
                }
            };
            _mockCosmosService.Setup(x => x.GetConnectorUsingPreRelease(testId, true)).ReturnsAsync(connectors);

            List<DataModel.DataSourceInformation> userRegistrations = new List<DataModel.DataSourceInformation>();
            userRegistrations.Add(new DataModel.DataSourceInformation() { AppCode = "RegisteredAppCode" });
            _mockOAuthService.Setup(x => x.GetUserRegistrations()).ReturnsAsync(userRegistrations);
            List<DataModel.DataSourceInformation> availableRegistrations = new List<DataModel.DataSourceInformation>();
            availableRegistrations.Add(new DataModel.DataSourceInformation() { AppCode = appCode, Domain = domain });
            _mockOAuthService.Setup(x => x.GetAvailableRegistrations()).ReturnsAsync(availableRegistrations);
            IConfiguration config = Utility.CreateConfiguration(AppSettings.OAuthUrl, "OAuthBaseUrl");
            var registrationService = CreateService(config);

            //Act
            DataModels.DataSourceRegistration register = await registrationService.RegisterConnector(testId);

            //Assert
            Assert.IsNotNull(register);
            Assert.IsFalse(register.Registered);
            Assert.IsNotNull(register.AuthorizeUrl);
        }

        [TestMethod]
        public async Task RegisterValidConnectorWithTenantWideRegistrationWithToken()
        {
            //Arrange
            var testId = "140";
            var appCode = "AppCode";
            var domain = "AppCodeDomain";
            var userId = Guid.NewGuid();
            var tenantId = Guid.NewGuid();
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockSessionAccessor.Setup(x => x.TenantId).Returns(tenantId);
            _mockOAuthApiRepository.Setup(x => x.GetToken(appCode, It.IsAny<string>())).ReturnsAsync(new OAuthAPITokenResponse());
            _mockOAuthService.Setup(x => x.AutomaticRegistration(appCode)).ReturnsAsync(true);

            AppConnectorModel connectors = new AppConnectorModel()
            {
                DataSource = new DataSourceModel()
                {
                    AppCode = appCode,
                    Domain = domain,
                    RegistrationInfo = new App.Model.RegistrationInfoModel()
                    { DomainRequired = true, RegistrationMode = RegistrationEntityMode.TenantWide }
                }
            };
            _mockCosmosService.Setup(x => x.GetConnectorUsingPreRelease(testId, true)).ReturnsAsync(connectors);

            List<DataModel.DataSourceInformation> userRegistrations = new List<DataModel.DataSourceInformation>();
            userRegistrations.Add(new DataModel.DataSourceInformation() { AppCode = "RegisteredAppCode" });
            _mockOAuthService.Setup(x => x.GetUserRegistrations()).ReturnsAsync(userRegistrations);
            List<DataModel.DataSourceInformation> availableRegistrations = new List<DataModel.DataSourceInformation>();
            availableRegistrations.Add(new DataModel.DataSourceInformation() { AppCode = appCode, Domain = domain });
            _mockOAuthService.Setup(x => x.GetAvailableRegistrations()).ReturnsAsync(availableRegistrations);
            var registrationService = CreateService();

            //Act
            DataModels.DataSourceRegistration register = await registrationService.RegisterConnector(testId);

            //Assert
            _mockOAuthService.Verify(x => x.AutomaticRegistration(appCode), times: Times.Once());
            Assert.IsNotNull(register);
            Assert.IsTrue(register.Registered);
        }

        [TestMethod]
        public async Task RegisterValidConnectorForAuthUrl()
        {
            //Arrange
            var testId = "140";
            var appCode = "PLCUS";
            var userId = new Guid("55e7a5d2-2134-4828-a2cd-2c4284ec11b9");
            var tenantId = new Guid("d564ff78-bdab-4bf4-c9ae-08d83232798c");
            _mockSessionAccessor.Setup(x => x.UserId).Returns(userId);
            _mockSessionAccessor.Setup(x => x.TenantId).Returns(tenantId);
            _mockOAuthService.Setup(x => x.AutomaticRegistration(appCode)).Returns(Task.FromResult<bool>(true));

            AppConnectorModel connectors = new AppConnectorModel()
            {
                DataSource = new DataSourceModel()
                {
                    AppCode = appCode,
                    Domain = "Domain1",
                    RegistrationInfo = new App.Model.RegistrationInfoModel()
                    { DomainRequired = true, RegistrationMode = RegistrationEntityMode.Manual }
                }
            };
            _mockCosmosService.Setup(x => x.GetConnectorUsingPreRelease(It.IsAny<string>(), true)).Returns(Task.FromResult(connectors));


            List<DataModel.DataSourceInformation> userRegistrations = new List<DataModel.DataSourceInformation>();
            userRegistrations.Add(new DataModel.DataSourceInformation() { AppCode = "PLCUK" });
            _mockOAuthService
                .Setup(x => x.GetUserRegistrations())
                .Returns(Task.FromResult<IEnumerable<DataModel.DataSourceInformation>>(userRegistrations.AsEnumerable()));

            List<DataModel.DataSourceInformation> availableRegistrations = new List<DataModel.DataSourceInformation>();
            availableRegistrations.Add(new DataModel.DataSourceInformation() { AppCode = appCode, Domain = "Domain1" });
            _mockOAuthService.Setup(x => x.GetAvailableRegistrations()).Returns(Task.FromResult(availableRegistrations));

            IConfiguration config = Utility.CreateConfiguration(AppSettings.OAuthUrl, "OAuthBaseUrl");
            var registrationService = CreateService(config);
            //Act
            DataModels.DataSourceRegistration register = await registrationService.RegisterConnector(testId);
            //Assert
            Assert.IsNotNull(register);
            Assert.IsFalse(register.Registered);
            Assert.IsNotNull(register.AuthorizeUrl);
        }

        [TestMethod]
        public async Task RegisterDataSource_returnsTrue_ifAlreadyRegistered()
        {
            //Arrange
            var appCode = "productToRegister";
            var productDomain = "";

            List<DataModel.DataSourceInformation> userRegistrations = new List<DataModel.DataSourceInformation>();
            userRegistrations.Add(new DataModel.DataSourceInformation() { AppCode = appCode });
            _mockOAuthService
                .Setup(x => x.GetUserRegistrations())
                .Returns(Task.FromResult<IEnumerable<DataModel.DataSourceInformation>>(userRegistrations.AsEnumerable()));

            var registrationService = CreateService();

            //Act
            DataModel.DataSourceRegistration register = await registrationService.RegisterDataSource(appCode, productDomain);

            //Assert
            _mockCosmosService.Verify(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>()), times: Times.Never());
            _mockOAuthService.Verify(x => x.GetAvailableRegistrations(), times: Times.Never());
            _mockOAuthService.Verify(x => x.AutomaticRegistration(It.IsAny<string>()), times: Times.Never());
            Assert.IsNotNull(register);
            Assert.IsTrue(register.Registered);
            Assert.IsNull(register.AuthorizeUrl);
        }

        [TestMethod]
        public async Task RegisterDataSource_returnsFalse_ifDataSourceDoesntExist()
        {
            //Arrange
            var appCode = "productToRegister";
            var productDomain = "";

            List<DataModel.DataSourceInformation> userRegistrations = new List<DataModel.DataSourceInformation>();
            _mockOAuthService
                .Setup(x => x.GetUserRegistrations())
                .Returns(Task.FromResult<IEnumerable<DataModel.DataSourceInformation>>(userRegistrations.AsEnumerable()));
            _mockCosmosService.Setup(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>()));

            var registrationService = CreateService();

            //Act
            DataModel.DataSourceRegistration register = await registrationService.RegisterDataSource(appCode, productDomain);

            //Assert
            _mockCosmosService.Verify(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>()), times: Times.Once());
            _mockOAuthService.Verify(x => x.GetAvailableRegistrations(), times: Times.Never());
            _mockOAuthService.Verify(x => x.AutomaticRegistration(It.IsAny<string>()), times: Times.Never());
            Assert.IsNotNull(register);
            Assert.IsFalse(register.Registered);
            Assert.IsNull(register.AuthorizeUrl);
        }

        [TestMethod]
        public async Task RegisterDataSource_returnsFalse_ifProductNotSetup()
        {
            //Arrange
            var appCode = "productToRegister";
            var productDomain = "";

            List<DataModel.DataSourceInformation> userRegistrations = new List<DataModel.DataSourceInformation>();
            _mockOAuthService
                .Setup(x => x.GetUserRegistrations())
                .Returns(Task.FromResult<IEnumerable<DataModel.DataSourceInformation>>(userRegistrations.AsEnumerable()));
            List<DataSourceModel> model = new List<DataSourceModel>() { new DataSourceModel() { AppCode = appCode, RegistrationInfo = new App.Model.RegistrationInfoModel() { DomainRequired = false } } };
            _mockCosmosService.Setup(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>())).Returns(Task.FromResult<IEnumerable<DataSourceModel>>(model));
            _mockOAuthService.Setup(x => x.GetAvailableRegistrations()).Returns(Task.FromResult(new List<DataModel.DataSourceInformation>()));

            var registrationService = CreateService();

            //Act
            DataModel.DataSourceRegistration register = await registrationService.RegisterDataSource(appCode, productDomain);

            //Assert
            _mockCosmosService.Verify(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>()), times: Times.Once());
            _mockOAuthService.Verify(x => x.GetAvailableRegistrations(), times: Times.Once());
            _mockOAuthService.Verify(x => x.AutomaticRegistration(It.IsAny<string>()), times: Times.Never());
            Assert.IsNotNull(register);
            Assert.IsFalse(register.Registered);
            Assert.IsNull(register.AuthorizeUrl);
        }

        [TestMethod]
        public async Task RegisterDataSource_returnsFalse_ifProductSetupButNoDomainPassed()
        {
            //Arrange
            var appCode = "productToRegister";
            var productDomain = "";

            List<DataModel.DataSourceInformation> userRegistrations = new List<DataModel.DataSourceInformation>();
            _mockOAuthService
                .Setup(x => x.GetUserRegistrations())
                .Returns(Task.FromResult<IEnumerable<DataModel.DataSourceInformation>>(userRegistrations.AsEnumerable()));
            List<DataSourceModel> model = new List<DataSourceModel>() { new DataSourceModel() { AppCode = appCode, RegistrationInfo = new App.Model.RegistrationInfoModel() { DomainRequired = true } } };
            _mockCosmosService.Setup(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>())).Returns(Task.FromResult<IEnumerable<DataSourceModel>>(model));
            List<DataModel.DataSourceInformation> dataSourceInformation = new List<DataModel.DataSourceInformation>() { new DataModel.DataSourceInformation() { AppCode = appCode } };
            _mockOAuthService.Setup(x => x.GetAvailableRegistrations()).Returns(Task.FromResult(dataSourceInformation));

            var registrationService = CreateService();

            //Act
            DataModel.DataSourceRegistration register = await registrationService.RegisterDataSource(appCode, productDomain);

            //Assert
            _mockCosmosService.Verify(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>()), times: Times.Once());
            _mockOAuthService.Verify(x => x.GetAvailableRegistrations(), times: Times.Never());
            _mockOAuthService.Verify(x => x.AutomaticRegistration(It.IsAny<string>()), times: Times.Never());
            Assert.IsNotNull(register);
            Assert.IsFalse(register.Registered);
            Assert.IsNull(register.AuthorizeUrl);
        }

        [TestMethod]
        public async Task RegisterDataSource_returnsFalse_ifProductSetup_AndAutomaticIncorrectDomain()
        {
            //Arrange
            var appCode = "productToRegister";
            var productDomain = "https://productToRegister.com";

            List<DataModel.DataSourceInformation> userRegistrations = new List<DataModel.DataSourceInformation>();
            _mockOAuthService
                .Setup(x => x.GetUserRegistrations())
                .Returns(Task.FromResult<IEnumerable<DataModel.DataSourceInformation>>(userRegistrations.AsEnumerable()));
            List<DataSourceModel> model = new List<DataSourceModel>() { new DataSourceModel() { AppCode = appCode, RegistrationInfo = new App.Model.RegistrationInfoModel() { RegistrationMode = RegistrationEntityMode.Automatic, DomainRequired = true } } };
            _mockCosmosService.Setup(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>())).Returns(Task.FromResult<IEnumerable<DataSourceModel>>(model));
            List<DataModel.DataSourceInformation> dataSourceInformation = new List<DataModel.DataSourceInformation>() { new DataModel.DataSourceInformation() { AppCode = appCode, Domain = "https://SomeOtherDomain.com" } };
            _mockOAuthService.Setup(x => x.GetAvailableRegistrations()).Returns(Task.FromResult(dataSourceInformation));
            _mockOAuthService.Setup(x => x.AutomaticRegistration(appCode)).Returns(Task.FromResult(true));

            var registrationService = CreateService();

            //Act
            DataModel.DataSourceRegistration register = await registrationService.RegisterDataSource(appCode, productDomain);

            //Assert
            _mockCosmosService.Verify(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>()), times: Times.Once());
            _mockOAuthService.Verify(x => x.GetAvailableRegistrations(), times: Times.Once());
            _mockOAuthService.Verify(x => x.AutomaticRegistration(It.IsAny<string>()), times: Times.Never());
            Assert.IsNotNull(register);
            Assert.IsFalse(register.Registered);
            Assert.IsNull(register.AuthorizeUrl);
        }

        [TestMethod]
        public async Task RegisterDataSource_returnsTrue_ifProductSetup_AndAutomaticCorrectDomain()
        {
            //Arrange
            var appCode = "productToRegister";
            var productDomain = "https://productToRegister.com";

            List<DataModel.DataSourceInformation> userRegistrations = new List<DataModel.DataSourceInformation>();
            _mockOAuthService
                .Setup(x => x.GetUserRegistrations())
                .Returns(Task.FromResult<IEnumerable<DataModel.DataSourceInformation>>(userRegistrations.AsEnumerable()));
            List<DataSourceModel> model = new List<DataSourceModel>() { new DataSourceModel() { AppCode = appCode, RegistrationInfo = new App.Model.RegistrationInfoModel() { RegistrationMode = RegistrationEntityMode.Automatic, DomainRequired = true } } };
            _mockCosmosService.Setup(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>())).Returns(Task.FromResult<IEnumerable<DataSourceModel>>(model));
            List<DataModel.DataSourceInformation> dataSourceInformation = new List<DataModel.DataSourceInformation>() { new DataModel.DataSourceInformation() { AppCode = appCode, Domain = productDomain } };
            _mockOAuthService.Setup(x => x.GetAvailableRegistrations()).Returns(Task.FromResult(dataSourceInformation));
            _mockOAuthService.Setup(x => x.AutomaticRegistration(appCode)).Returns(Task.FromResult(true));

            var registrationService = CreateService();

            //Act
            DataModel.DataSourceRegistration register = await registrationService.RegisterDataSource(appCode, productDomain);

            //Assert
            _mockCosmosService.Verify(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>()), times: Times.Once());
            _mockOAuthService.Verify(x => x.GetAvailableRegistrations(), times: Times.Once());
            _mockOAuthService.Verify(x => x.AutomaticRegistration(It.IsAny<string>()), times: Times.Once());
            Assert.IsNotNull(register);
            Assert.IsTrue(register.Registered);
            Assert.IsNull(register.AuthorizeUrl);
        }

        [TestMethod]
        public async Task RegisterDataSource_returnsTrue_ifProductSetup_AndAutomatic()
        {
            //Arrange
            var appCode = "productToRegister";
            var productDomain = "";

            List<DataModel.DataSourceInformation> userRegistrations = new List<DataModel.DataSourceInformation>();
            _mockOAuthService
                .Setup(x => x.GetUserRegistrations())
                .Returns(Task.FromResult<IEnumerable<DataModel.DataSourceInformation>>(userRegistrations.AsEnumerable()));
            List<DataSourceModel> model = new List<DataSourceModel>() { new DataSourceModel() { AppCode = appCode, RegistrationInfo = new App.Model.RegistrationInfoModel() { RegistrationMode = RegistrationEntityMode.Automatic } } };
            _mockCosmosService.Setup(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>())).Returns(Task.FromResult<IEnumerable<DataSourceModel>>(model));
            List<DataModel.DataSourceInformation> dataSourceInformation = new List<DataModel.DataSourceInformation>() { new DataModel.DataSourceInformation() { AppCode = appCode } };
            _mockOAuthService.Setup(x => x.GetAvailableRegistrations()).Returns(Task.FromResult(dataSourceInformation));
            _mockOAuthService.Setup(x => x.AutomaticRegistration(appCode)).Returns(Task.FromResult(true));

            var registrationService = CreateService();

            //Act
            DataModel.DataSourceRegistration register = await registrationService.RegisterDataSource(appCode, productDomain);

            //Assert
            _mockCosmosService.Verify(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>()), times: Times.Once());
            _mockOAuthService.Verify(x => x.GetAvailableRegistrations(), times: Times.Once());
            _mockOAuthService.Verify(x => x.AutomaticRegistration(It.IsAny<string>()), times: Times.Once());
            Assert.IsNotNull(register);
            Assert.IsTrue(register.Registered);
            Assert.IsNull(register.AuthorizeUrl);
        }

        [TestMethod]
        public async Task RegisterDataSource_returnsFalse_ifProductSetup_AndManual()
        {
            //Arrange
            var appCode = "productToRegister";
            var productDomain = "";
            var fakeOAuthDomain = "https://OAuth.thomsonreuters.com/";

            List<DataModel.DataSourceInformation> userRegistrations = new List<DataModel.DataSourceInformation>();
            _mockOAuthService
                .Setup(x => x.GetUserRegistrations())
                .Returns(Task.FromResult<IEnumerable<DataModel.DataSourceInformation>>(userRegistrations.AsEnumerable()));
            List<DataSourceModel> model = new List<DataSourceModel>() { new DataSourceModel() { AppCode = appCode, RegistrationInfo = new App.Model.RegistrationInfoModel() { RegistrationMode = RegistrationEntityMode.Manual } } };
            _mockCosmosService.Setup(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>())).Returns(Task.FromResult<IEnumerable<DataSourceModel>>(model));
            List<DataModel.DataSourceInformation> dataSourceInformation = new List<DataModel.DataSourceInformation>() { new DataModel.DataSourceInformation() { AppCode = appCode } };
            _mockOAuthService.Setup(x => x.GetAvailableRegistrations()).Returns(Task.FromResult(dataSourceInformation));
            _mockOAuthService.Setup(x => x.AutomaticRegistration(appCode)).Returns(Task.FromResult(true));
            IConfiguration config = Utility.CreateConfiguration(AppSettings.OAuthUrl, fakeOAuthDomain);

            var registrationService = CreateService(config);

            //Act
            DataModel.DataSourceRegistration register = await registrationService.RegisterDataSource(appCode, productDomain);

            //Assert
            _mockCosmosService.Verify(x => x.GetDataSources(It.IsAny<Expression<Func<DataSourceDocument, bool>>>()), times: Times.Once());
            _mockOAuthService.Verify(x => x.GetAvailableRegistrations(), times: Times.Once());
            _mockOAuthService.Verify(x => x.AutomaticRegistration(It.IsAny<string>()), times: Times.Never());
            Assert.IsNotNull(register);
            Assert.IsFalse(register.Registered);
            Assert.IsNotNull(register.AuthorizeUrl);
            Assert.IsTrue(register.AuthorizeUrl.StartsWith(fakeOAuthDomain));
        }
    }


}
