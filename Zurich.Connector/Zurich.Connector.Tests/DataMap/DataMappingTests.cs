using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Services.Security;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.ProductData.Models;
using AutoMapper;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Common.Repositories.Cosmos;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Data.Factories;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Zurich.Connector.Data;

namespace Zurich.Connector.Tests
{
	[TestClass]
	public class DataMappingTests
	{
		private Mock<IRepository> _mockRepository;
		private Mock<IDataMappingRepository> _mockDataMappingRepository;
		private Mock<IOAuthService> _mockOAuthService;
		private Mock<ILogger<DataMappingOAuth>> _mockLoggerOAuth;
		private Mock<ILogger<DataMappingTransfer>> _mockLoggerTransfer;
		private Mock<ConnectorCosmosContext> _mockCosmosDocumentReader;
		private Mock<IHttpBodyFactory> _mockHttpBodyFactory;
		private Mock<IHttpResponseFactory> _mockHttpResponseFactory;
		private Mock<IMapper> _mockMapper;
		private OAuthOptions _fakeOAuthOptions;
        private Mock<IHttpContextAccessor> _mockContextAccessor;
        private Mock<IOAuthApiRepository> _mockOAuthApirepository;
		private Mock<LegalHomeAccessCheck> _mockLegalHomeAccessCheck;

        [TestInitialize]
		public void TestInitialize()
		{
			_mockRepository = new Mock<IRepository>();
			_mockDataMappingRepository = new Mock<IDataMappingRepository>();
			_mockOAuthService = new Mock<IOAuthService>();
			_mockLoggerOAuth = new Mock<ILogger<DataMappingOAuth>>();
			_mockLoggerTransfer = new Mock<ILogger<DataMappingTransfer>>();
			_mockHttpBodyFactory = new Mock<IHttpBodyFactory>();
			_mockHttpResponseFactory = new Mock<IHttpResponseFactory>();
			_fakeOAuthOptions = new OAuthOptions();
            _mockContextAccessor = new Mock<IHttpContextAccessor>();
            _mockOAuthApirepository = new Mock<IOAuthApiRepository>();
			_mockLegalHomeAccessCheck = new Mock<LegalHomeAccessCheck>();

            _fakeOAuthOptions.Connections = new Dictionary<string, OAuthConnection>();

			// feels like this won't change
			AppToken token = new AppToken() { access_token = "fakeToken" };
			_mockOAuthService.Setup(x => x.GetToken(It.IsAny<string>(), It.IsAny<OAuthApplicationType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<ProductType>())).Returns(Task.FromResult(token));
			_mockOAuthApirepository.Setup(x => x.GetToken(It.IsAny<string>())).Returns(Task.FromResult(token));
			_mockCosmosDocumentReader = new Mock<ConnectorCosmosContext>(null, null);
			_mockMapper = new Mock<IMapper>();
		}

		#region json Strings

		private const string TwoDocumentsJson = @"
		{
			""data"": {
				""results"": [{
						""author_description"": ""Ryan Hunecke"",
						""author"": ""RYAN.HUNECKE"",
						""checksum"": ""SHA256:601CB91FD88ABF97135C25A25CC078A668AE40AFD55B393358BC6C5B0C41B5C4"",
						""class"": ""DOC"",
						""class_description"": ""Document"",
						""content_type"": ""D"",
						""create_date"": ""2021-02-05T22:09:07Z"",
						""database"": ""DATABASE2"",
						""default_security"": ""public"",
						""document_number"": 1678,
						""edit_date"": ""2021-02-05T22:09:07Z"",
						""edit_profile_date"": ""2021-02-05T22:09:41.386Z"",
						""file_create_date"": ""2021-02-05T22:09:07Z"",
						""file_edit_date"": ""2021-02-05T22:09:07Z"",
						""id"": ""Database2!1678.1"",
						""is_checked_out"": false,
						""is_declared"": false,
						""is_external"": false,
						""is_external_as_normal"": false,
						""is_hipaa"": false,
						""is_in_use"": false,
						""is_restorable"": true,
						""iwl"": ""iwl:dms=cloudimanage.com&&lib=Database2&&num=1678&&ver=1"",
						""last_user"": ""RYAN.HUNECKE"",
						""last_user_description"": ""Ryan Hunecke"",
						""name"": ""TestWordDoc"",
						""operator"": ""RYAN.HUNECKE"",
						""operator_description"": ""Ryan Hunecke"",
						""retain_days"": 10,
						""size"": 18733,
						""subclass_description"": """",
						""type"": ""WORDX"",
						""type_description"": ""WORD 2007"",
						""extension"": ""DOCX"",
						""version"": 1,
						""workspace_name"": ""Matter1"",
						""workspace_id"": ""Database2!3650"",
						""wstype"": ""document"",
						""complexObject"": {
							""testItem1"": ""fakeDescription1"",
							""testItem2"": ""fakeDescription2"",
							""testItem3"": {
								""level2"": ""level 2 variable""
							}
						},
						""testArray"": [
							{
								""name"": ""testIndex1"",
								""description"": ""fakeDesc""
							},
							{
								""name"": ""testIndex2"",
								""description"": ""fakeDesc2""
							},
							{
								""name"": ""testIndex3"",
								""description"": ""fakeDesc3""
							}
						],
						""wstype"": ""document""
					}, {
						""author_description"": ""Sally Sales"",
						""author"": ""ALEX.PRICE"",
						""checksum"": ""SHA256:278E508D78553AAF26314909CAD63FC75C16AB65A101FAF147DC08163508F762"",
						""class"": ""DOC"",
						""class_description"": ""Document"",
						""content_type"": ""D"",
						""create_date"": ""2021-01-28T14:56:11.915Z"",
						""database"": ""CONTRACTEXPRESS"",
						""default_security"": ""private"",
						""document_number"": 1840,
						""edit_date"": ""2021-01-28T14:56:11.915Z"",
						""edit_profile_date"": ""2021-01-28T14:56:11.915Z"",
						""file_create_date"": ""2021-01-28T14:56:11.915Z"",
						""file_edit_date"": ""2021-01-28T14:56:11.915Z"",
						""id"": ""ContractExpress!1840.1"",
						""is_checked_out"": false,
						""is_declared"": false,
						""is_external"": false,
						""is_external_as_normal"": false,
						""is_hipaa"": false,
						""is_in_use"": false,
						""is_restorable"": true,
						""iwl"": ""iwl:dms=cloudimanage.com&&lib=ContractExpress&&num=1840&&ver=1"",
						""last_user"": ""ALEX.PRICE"",
						""last_user_description"": ""Sally Sales"",
						""name"": ""test"",
						""operator"": ""ALEX.PRICE"",
						""operator_description"": ""Sally Sales"",
						""retain_days"": 10,
						""size"": 11980,
						""subclass_description"": """",
						""type_description"": ""WORD 2007"",
						""type"": ""WORDX"",
						""extension"": ""DOCX"",
						""version"": 1,
						""workspace_name"": ""TestMy Matter"",
						""workspace_id"": ""ContractExpress!1605"",
						""wstype"": ""document""

					}
				]
			}
		}";

		private const string TwoDocumentsListJson = @"
		[{
						""author_description"": ""Ryan Hunecke"",
						""author"": ""RYAN.HUNECKE"",
						""checksum"": ""SHA256:601CB91FD88ABF97135C25A25CC078A668AE40AFD55B393358BC6C5B0C41B5C4"",
						""class"": ""DOC"",
						""class_description"": ""Document"",
						""content_type"": ""D"",
						""create_date"": ""2021-02-05T22:09:07Z"",
						""database"": ""DATABASE2"",
						""default_security"": ""public"",
						""document_number"": 1678,
						""edit_date"": ""2021-02-05T22:09:07Z"",
						""edit_profile_date"": ""2021-02-05T22:09:41.386Z"",
						""file_create_date"": ""2021-02-05T22:09:07Z"",
						""file_edit_date"": ""2021-02-05T22:09:07Z"",
						""id"": ""Database2!1678.1"",
						""is_checked_out"": false,
						""is_declared"": false,
						""is_external"": false,
						""is_external_as_normal"": false,
						""is_hipaa"": false,
						""is_in_use"": false,
						""is_restorable"": true,
						""iwl"": ""iwl:dms=cloudimanage.com&&lib=Database2&&num=1678&&ver=1"",
						""last_user"": ""RYAN.HUNECKE"",
						""last_user_description"": ""Ryan Hunecke"",
						""name"": ""TestWordDoc"",
						""operator"": ""RYAN.HUNECKE"",
						""operator_description"": ""Ryan Hunecke"",
						""retain_days"": 10,
						""size"": 18733,
						""subclass_description"": """",
						""type"": ""WORDX"",
						""type_description"": ""WORD 2007"",
						""extension"": ""DOCX"",
						""version"": 1,
						""workspace_name"": ""Matter1"",
						""workspace_id"": ""Database2!3650"",
						""wstype"": ""document""
					}, {
						""author_description"": ""Sally Sales"",
						""author"": ""ALEX.PRICE"",
						""checksum"": ""SHA256:278E508D78553AAF26314909CAD63FC75C16AB65A101FAF147DC08163508F762"",
						""class"": ""DOC"",
						""class_description"": ""Document"",
						""content_type"": ""D"",
						""create_date"": ""2021-01-28T14:56:11.915Z"",
						""database"": ""CONTRACTEXPRESS"",
						""default_security"": ""private"",
						""document_number"": 1840,
						""edit_date"": ""2021-01-28T14:56:11.915Z"",
						""edit_profile_date"": ""2021-01-28T14:56:11.915Z"",
						""file_create_date"": ""2021-01-28T14:56:11.915Z"",
						""file_edit_date"": ""2021-01-28T14:56:11.915Z"",
						""id"": ""ContractExpress!1840.1"",
						""is_checked_out"": false,
						""is_declared"": false,
						""is_external"": false,
						""is_external_as_normal"": false,
						""is_hipaa"": false,
						""is_in_use"": true,
						""is_restorable"": true,
						""iwl"": ""iwl:dms=cloudimanage.com&&lib=ContractExpress&&num=1840&&ver=1"",
						""last_user"": ""ALEX.PRICE"",
						""last_user_description"": ""Sally Sales"",
						""name"": ""test"",
						""operator"": ""ALEX.PRICE"",
						""operator_description"": ""Sally Sales"",
						""retain_days"": 10,
						""size"": 11980,
						""subclass_description"": """",
						""type_description"": ""WORD 2007"",
						""type"": ""WORDX"",
						""extension"": ""DOCX"",
						""version"": 1,
						""workspace_name"": ""TestMy Matter"",
						""workspace_id"": ""ContractExpress!1605"",
						""wstype"": ""document""

					}
				]";

		private const string userInfoJson = @"
		{
			""name"": ""Ryan Hunecke"",
			""id"": ""241"",
		}";

		#endregion

		[TestMethod]
		public async Task TestMapping()
		{
			// ARRANGE
			string appCode = "TestApp";

			_mockRepository.Setup(x => x.MakeRequest(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>(), It.IsAny<string>())).Returns(Task.FromResult(TwoDocumentsJson));

			ConnectorDocument connectorDocument = new ConnectorDocument()
			{
				ResultLocation = "data.results",
				Request = new ConnectorRequest()
				{ EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
                {
					Type = ResponseContentType.JSON
                }
			};
			connectorDocument.CdmMapping = new CDMMapping()
			{
				structured = new List<CDMElement>() {
							{
								new CDMElement(){  name = "Name", responseElement ="name"}
							},
							{
								new CDMElement(){ name="Id", responseElement ="id"}
							},
							{
								new CDMElement(){ name="WebLink", responseElement =""}
							},
							{
								new CDMElement(){ name="LastOpened", responseElement =""}
							},
							{
								new CDMElement(){ name="LastUpdated", responseElement ="file_edit_date"}
							},
							{
								new CDMElement(){ name="LastModifiedUser", responseElement ="last_user_description"}
							},
							{
								new CDMElement(){ name="CreatedDate", responseElement ="file_create_date"}
							},
							{
								new CDMElement(){ name="CreatedByUser", responseElement ="author_description"}
							},
							{
								new CDMElement(){ name="FileType", responseElement ="type"}
							},
							{
								new CDMElement(){ name="FileTypeExtended", responseElement ="type_description"}
							}
				}
			};

			connectorDocument.DataSource = new DataSourceDocument()
			{
				appCode = appCode,
				securityDefinition = new SecurityDefinition()
				{
					defaultSecurityDefinition = new SecurityDefinitionDetails()
					{
						authorizationHeader = "differentAuthHeader"
					}
				}
			};

			Mock<IHttpBodyService> mockBodyService = new Mock<IHttpBodyService>();
			_mockHttpBodyFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockBodyService.Object);

			Mock<IHttpResponseService> mockResponseService = new Mock<IHttpResponseService>();
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(JToken.Parse(TwoDocumentsJson));
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);

			DataMappingOAuth documentMap = new DataMappingOAuth(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerOAuth.Object, _mockCosmosDocumentReader.Object, _mockMapper.Object, _mockHttpBodyFactory.Object, _mockHttpResponseFactory.Object, _mockContextAccessor.Object, _mockOAuthApirepository.Object, _fakeOAuthOptions, _mockLegalHomeAccessCheck.Object);

			// ACT
			dynamic documents = await documentMap.GetAndMapResults<dynamic>(connectorDocument, null);

			// ASSERT
			Assert.IsNotNull(documents);
			Assert.AreEqual(2, documents.Count);
		}

		[TestMethod]
		public async Task TestMappingNoresultLocation()
		{
			// ARRANGE
			string appCode = "TestApp";

			_mockRepository.Setup(x => x.MakeRequest(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>(), It.IsAny<string>())).Returns(Task.FromResult(TwoDocumentsListJson));

			ConnectorDocument connectorDocument = new ConnectorDocument()
			{
				Request = new ConnectorRequest()
				{ EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON
				}
			};
			connectorDocument.CdmMapping = new CDMMapping()
			{
				structured = new List<CDMElement>() {
							{
								new CDMElement(){ name="Name", responseElement ="name"}
							},
							{
								new CDMElement(){ name="Id", responseElement ="id"}
							},
							{
								new CDMElement(){ name="WebLink", responseElement =""}
							},
							{
								new CDMElement(){ name="LastOpened", responseElement =""}
							},
							{
								new CDMElement(){ name="LastUpdated", responseElement ="file_edit_date"}
							},
							{
								new CDMElement(){ name="LastModifiedUser", responseElement ="last_user_description"}
							},
							{
								new CDMElement(){ name="CreatedDate", responseElement ="file_create_date"}
							},
							{
								new CDMElement(){ name="CreatedByUser", responseElement ="author_description"}
							},
							{
								new CDMElement(){ name="FileType", responseElement ="type"}
							},
							{
								new CDMElement(){ name="FileTypeExtended", responseElement ="type_description"}
							}
				}
			};

			connectorDocument.DataSource = new DataSourceDocument()
			{
				appCode = appCode,
				securityDefinition = new SecurityDefinition()
				{
					defaultSecurityDefinition = new SecurityDefinitionDetails()
					{
						authorizationHeader = "differentAuthHeader"
					}
				}
			};


			Mock<IHttpBodyService> mockBodyService = new Mock<IHttpBodyService>();
			_mockHttpBodyFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockBodyService.Object);

			Mock<IHttpResponseService> mockResponseService = new Mock<IHttpResponseService>();
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(JToken.Parse(TwoDocumentsListJson));
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);

			DataMappingOAuth documentMap = new DataMappingOAuth(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerOAuth.Object, _mockCosmosDocumentReader.Object, _mockMapper.Object, _mockHttpBodyFactory.Object, _mockHttpResponseFactory.Object, _mockContextAccessor.Object, _mockOAuthApirepository.Object, _fakeOAuthOptions, _mockLegalHomeAccessCheck.Object);

			// ACT
			dynamic documents = await documentMap.GetAndMapResults<dynamic>(connectorDocument);

			// ASSERT
			Assert.IsNotNull(documents);
			Assert.AreEqual(2, documents.Count);
		}

		[TestMethod]
		public async Task TestMappingWithComplexObjectAndArray()
		{
			// ARRANGE
			string appCode = "TestApp";

			_mockRepository.Setup(x => x.MakeRequest(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>(), It.IsAny<string>())).Returns(Task.FromResult(TwoDocumentsJson));

			ConnectorDocument connectorDocument = new ConnectorDocument()
			{
				ResultLocation = "data.results",
				Request = new ConnectorRequest()
				{ EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON
				}
			};
			connectorDocument.CdmMapping = new CDMMapping()
			{
				structured = new List<CDMElement>() {
							{
								new CDMElement(){ name="Name", responseElement ="name"}
							},
							{
								new CDMElement(){ name="Id", responseElement ="id"}
							},
							{
								new CDMElement(){ name="WebLink", responseElement =""}
							},
							{
								new CDMElement(){ name="ComplexObjectFieldOne", responseElement ="complexObject.testItem1"}
							},
							{
								new CDMElement(){ name="ComplexObjectLevelTwo", responseElement ="complexObject.testItem3.level2"}
							},
							{
								new CDMElement(){ name="ArrayValue1", responseElement ="testArray.[name:testIndex2].description"}
							},
							{
								new CDMElement(){ name="ArrayValue2", responseElement ="testArray.[description:fakeDesc3].name"}
							}
				}
			};

			connectorDocument.DataSource = new DataSourceDocument()
			{
				appCode = appCode,
				securityDefinition = new SecurityDefinition()
				{
					defaultSecurityDefinition = new SecurityDefinitionDetails()
					{
						authorizationHeader = "differentAuthHeader"
					}
				}
			};

			Mock<IHttpBodyService> mockBodyService = new Mock<IHttpBodyService>();
			_mockHttpBodyFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockBodyService.Object);

			Mock<IHttpResponseService> mockResponseService = new Mock<IHttpResponseService>();
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(JToken.Parse(TwoDocumentsJson));
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);

			DataMappingOAuth documentMap = new DataMappingOAuth(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerOAuth.Object, _mockCosmosDocumentReader.Object, _mockMapper.Object, _mockHttpBodyFactory.Object, _mockHttpResponseFactory.Object, _mockContextAccessor.Object, _mockOAuthApirepository.Object, _fakeOAuthOptions, _mockLegalHomeAccessCheck.Object);

			// ACT
			dynamic documents = await documentMap.GetAndMapResults<dynamic>(connectorDocument, "fakeTransferToken");

			// ASSERT
			Assert.IsNotNull(documents);
			Assert.AreEqual(2, documents.Count);
			Assert.AreEqual("fakeDescription1", documents[0].ComplexObjectFieldOne.ToString());
			Assert.AreEqual("level 2 variable", documents[0].ComplexObjectLevelTwo.ToString());
			Assert.AreEqual("fakeDesc2", documents[0].ArrayValue1.ToString());
			Assert.AreEqual("testIndex3", documents[0].ArrayValue2.ToString());
		}

        [TestMethod]
        public async Task TestMappingWithNestedArray()
        {
            // ARRANGE
            string appCode = "TestApp";

            _mockRepository.Setup(x => x.MakeRequest(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>(), It.IsAny<string>())).Returns(Task.FromResult(TwoDocumentsJson));

            ConnectorDocument connectorDocument1 = new ConnectorDocument()
            {
                Id = "1",
                ResultLocation = "data.results",
                Request = new ConnectorRequest()
                { EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON
				}
			};
            connectorDocument1.CdmMapping = new CDMMapping()
            {
                structured = new List<CDMElement>() {
                            {
                                new CDMElement(){ name="Name", responseElement ="name"}
                            },
                            {
                                new CDMElement(){ name="Id", responseElement ="id"}
                            },
                            {
                                new CDMElement(){ name="WebLink", responseElement =""}
                            },
                            {
                                new CDMElement(){ name="ObjectArray", responseElement ="{2}"}
                            }
                }
            };

            connectorDocument1.DataSource = new DataSourceDocument()
            {
                appCode = appCode,
                securityDefinition = new SecurityDefinition()
                {
                    defaultSecurityDefinition = new SecurityDefinitionDetails()
                    {
                        authorizationHeader = "differentAuthHeader"
                    }
                }
            };

            ConnectorDocument connectorDocument2 = new ConnectorDocument()
            {
                Id = "2",
                ResultLocation = "testArray",
                Request = new ConnectorRequest()
                { EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON
				}
			};
            connectorDocument2.CdmMapping = new CDMMapping()
            {
                structured = new List<CDMElement>() {
                            {
                                new CDMElement(){ name="Name", responseElement ="name"}
                            },
                            {
                                new CDMElement(){ name="DescriptionProp", responseElement ="description"}
                            }
                }
            };

            connectorDocument2.DataSource = new DataSourceDocument()
            {
                appCode = appCode,
                securityDefinition = new SecurityDefinition()
                {
                    defaultSecurityDefinition = new SecurityDefinitionDetails()
                    {
                        authorizationHeader = "differentAuthHeader"
                    }
                }
            };
			_mockCosmosDocumentReader.Setup(x => x.GetDocument<ConnectorDocument>(CosmosConstants.ConnectorContainerId, "1",
				CosmosConstants.ConnectorPartitionKey)).Returns(Task.FromResult(connectorDocument1));

			_mockCosmosDocumentReader.Setup(x => x.GetDocument<ConnectorDocument>(CosmosConstants.ConnectorContainerId, "2",
				CosmosConstants.ConnectorPartitionKey)).Returns(Task.FromResult(connectorDocument2));

			Mock<IHttpBodyService> mockBodyService = new Mock<IHttpBodyService>();
			_mockHttpBodyFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockBodyService.Object);

			Mock<IHttpResponseService> mockResponseService = new Mock<IHttpResponseService>();
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(JToken.Parse(TwoDocumentsJson));
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);

			DataMappingOAuth documentMap = new DataMappingOAuth(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerOAuth.Object, _mockCosmosDocumentReader.Object, _mockMapper.Object, _mockHttpBodyFactory.Object, _mockHttpResponseFactory.Object, _mockContextAccessor.Object, _mockOAuthApirepository.Object, _fakeOAuthOptions, _mockLegalHomeAccessCheck.Object);

			// ACT
			dynamic documents = await documentMap.GetAndMapResults<dynamic>(connectorDocument1, "fakeTransferToken");

            // ASSERT
            Assert.IsNotNull(documents);
            Assert.AreEqual(2, documents.Count);
            Assert.AreEqual(3, documents[0].ObjectArray.Count);
            Assert.AreEqual("testIndex2", documents[0].ObjectArray[1].Name.ToString());
            Assert.AreEqual("fakeDesc2", documents[0].ObjectArray[1].DescriptionProp.ToString());
        }

		[TestMethod]
		public async Task TestMappingWithLegalHomeAccess()
		{
			// ARRANGE
			string appCode = "TestApp";

			_mockRepository.Setup(x => x.MakeRequest(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>(), It.IsAny<string>())).Returns(Task.FromResult(TwoDocumentsListJson));

			ConnectorDocument connectorDocument = new ConnectorDocument()
			{
				Request = new ConnectorRequest()
				{ EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON
				}
			};
			connectorDocument.CdmMapping = new CDMMapping()
			{
				structured = new List<CDMElement>() {
							{
								new CDMElement(){ name="Name", responseElement ="name"}
							},
							{
								new CDMElement(){ name="Id", responseElement ="id"}
							},
							{
								new CDMElement(){ name="WebLink", responseElement =""}
							},
							{
								new CDMElement(){ name="LastOpened", responseElement =""}
							},
							{
								new CDMElement(){ name="LastUpdated", responseElement ="file_edit_date"}
							},
							{
								new CDMElement(){ name="LastModifiedUser", responseElement ="last_user_description"}
							},
							{
								new CDMElement(){ name="CreatedDate", responseElement ="file_create_date"}
							},
							{
								new CDMElement(){ name="CreatedByUser", responseElement ="author_description"}
							},
							{
								new CDMElement(){ name="FileType", responseElement ="type"}
							},
							{
								new CDMElement(){ name="FileTypeExtended", responseElement ="type_description"}
							}
				}
			};

			connectorDocument.DataSource = new DataSourceDocument()
			{
				appCode = appCode,
				securityDefinition = new SecurityDefinition()
				{
					defaultSecurityDefinition = new SecurityDefinitionDetails()
					{
						authorizationHeader = "differentAuthHeader"
					}
				}
			};


			Mock<IHttpBodyService> mockBodyService = new Mock<IHttpBodyService>();
			_mockHttpBodyFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockBodyService.Object);

			Mock<IHttpResponseService> mockResponseService = new Mock<IHttpResponseService>();
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(JToken.Parse(TwoDocumentsListJson));
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);
			Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
			var claims = new ClaimsPrincipal();
			var claimsIdent = new ClaimsIdentity(new List<Claim>() { new Claim("scope", DataConstants.LegalHomeScope) });
			claims.AddIdentity(claimsIdent);
			mockHttpContext.Setup(x => x.User).Returns(claims);
			_mockContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);
			DataMappingOAuth documentMap = new DataMappingOAuth(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerOAuth.Object, _mockCosmosDocumentReader.Object, _mockMapper.Object, _mockHttpBodyFactory.Object, _mockHttpResponseFactory.Object, _mockContextAccessor.Object, _mockOAuthApirepository.Object, _fakeOAuthOptions, _mockLegalHomeAccessCheck.Object);

			// ACT
			dynamic documents = await documentMap.GetAndMapResults<dynamic>(connectorDocument);

			// ASSERT
			Assert.IsNotNull(documents);
			Assert.AreEqual(2, documents.Count);
			//_mockOAuthService.Verify(x => x.GetToken(It.IsAny<string>(), It.IsAny<OAuthApplicationType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<ProductType>()), Times.Once());
		}

		[TestMethod]
		public async Task TestMappingWithoutLegalHomeAccess()
		{
			// ARRANGE
			string appCode = "TestApp";

			_mockRepository.Setup(x => x.MakeRequest(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>(), It.IsAny<string>())).Returns(Task.FromResult(TwoDocumentsListJson));

			ConnectorDocument connectorDocument = new ConnectorDocument()
			{
				Request = new ConnectorRequest()
				{ EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON
				}
			};
			connectorDocument.CdmMapping = new CDMMapping()
			{
				structured = new List<CDMElement>() {
							{
								new CDMElement(){ name="Name", responseElement ="name"}
							},
							{
								new CDMElement(){ name="Id", responseElement ="id"}
							},
							{
								new CDMElement(){ name="WebLink", responseElement =""}
							},
							{
								new CDMElement(){ name="LastOpened", responseElement =""}
							},
							{
								new CDMElement(){ name="LastUpdated", responseElement ="file_edit_date"}
							},
							{
								new CDMElement(){ name="LastModifiedUser", responseElement ="last_user_description"}
							},
							{
								new CDMElement(){ name="CreatedDate", responseElement ="file_create_date"}
							},
							{
								new CDMElement(){ name="CreatedByUser", responseElement ="author_description"}
							},
							{
								new CDMElement(){ name="FileType", responseElement ="type"}
							},
							{
								new CDMElement(){ name="FileTypeExtended", responseElement ="type_description"}
							}
				}
			};

			connectorDocument.DataSource = new DataSourceDocument()
			{
				appCode = appCode,
				securityDefinition = new SecurityDefinition()
				{
					defaultSecurityDefinition = new SecurityDefinitionDetails()
					{
						authorizationHeader = "differentAuthHeader"
					}
				}
			};


			Mock<IHttpBodyService> mockBodyService = new Mock<IHttpBodyService>();
			_mockHttpBodyFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockBodyService.Object);

			Mock<IHttpResponseService> mockResponseService = new Mock<IHttpResponseService>();
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(JToken.Parse(TwoDocumentsListJson));
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);
		
			DataMappingOAuth documentMap = new DataMappingOAuth(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerOAuth.Object, _mockCosmosDocumentReader.Object, _mockMapper.Object, _mockHttpBodyFactory.Object, _mockHttpResponseFactory.Object, _mockContextAccessor.Object, _mockOAuthApirepository.Object, _fakeOAuthOptions, _mockLegalHomeAccessCheck.Object);

			// ACT
			dynamic documents = await documentMap.GetAndMapResults<dynamic>(connectorDocument);

			// ASSERT
			Assert.IsNotNull(documents);
			Assert.AreEqual(2, documents.Count);
		}
	}
}
