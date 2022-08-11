using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Zurich.Common.Models.OAuth;
using Zurich.Common.Repositories;
using Zurich.Common.Services.Security;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Tests.Common;
using Zurich.ProductData.Models;

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
		private Mock<ILegalHomeAccessCheck> _mockLegalHomeAccessCheck;
		private IConfiguration _fakeConfiguration;
		private ICosmosService _cosmosService;
		private Mock<IOAuthServices> _mockOAuthServices;

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
			_mockLegalHomeAccessCheck = new Mock<ILegalHomeAccessCheck>();
			_fakeConfiguration = Utility.CreateConfiguration("fakeKey", "fakeValue");

			_fakeOAuthOptions.Connections = new Dictionary<string, OAuthConnection>();

			// feels like this won't change
			OAuthAPITokenResponse token = new OAuthAPITokenResponse() { AccessToken = "fakeToken" };
			AppToken appToken = new AppToken() { access_token = "fakeToken" };
			_mockOAuthService.Setup(x => x.GetToken(It.IsAny<string>(), It.IsAny<OAuthApplicationType>(), It.IsAny<string>(), It.IsAny<ProductType>())).Returns(Task.FromResult(appToken));
			_mockOAuthApirepository.Setup(x => x.GetToken(It.IsAny<string>())).Returns(Task.FromResult(token));
			_mockCosmosDocumentReader = new Mock<ConnectorCosmosContext>(null, null);
			_mockMapper = new Mock<IMapper>();
			_cosmosService = new CosmosService(_mockCosmosDocumentReader.Object, _mockMapper.Object, _fakeConfiguration);
			_mockOAuthServices = new Mock<IOAuthServices>();

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
						""boolTest"": ""true"",
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
						""boolTest"": ""true"",
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

		private const string OneDocumentJson = @"{
			""data"": {
				""results"": [{
						""author_description"": ""Ryan Hunecke"",
						""author"": ""RYAN.HUNECKE"",
						""checksum"": ""SHA256:601CB91FD88ABF97135C25A25CC078A668AE40AFD55B393358BC6C5B0C41B5C4"",
						""class"": ""DOC"",
						""class_description"": ""Document"",
						""content_type"": ""D"",
						""create_date"": ""20150421"",
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
						""boolTest"": ""true"",
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
					}
				]
			},
         ""Count"": 1,
		 ""Documents"": [{
							""title"": ""Extradition: special cases"",
							""webURL"": ""https://uk.westlaw.com/Document/I111C8210486311E3BB9A89B1597EA15E/View/FullText.html?navigationPath=Search%2Fv1%2Fresults%2Fnavigation%2Fi0ad6ad3b0000018149e3359b18a1486c%3Fppcid%3Di0115653351ea42de9d0a835d3715bac6%26Nav%3DRESEARCH_COMBINED_WLUK%26fragmentIdentifier%3DI111C8210486311E3BB9A89B1597EA15E%26parentRank%3D0%26startIndex%3D6%26transitionType%3DSearchItem&amp;listSource=Search&amp;list=RESEARCH_COMBINED_WLUK&amp;rank=6&amp;ppcid=i0115653351ea42de9d0a835d3715bac6&amp;originationContext=Search%20Result&amp;transitionType=SearchItem"",
							""type"": ""Overviews"",
							""creationDate"": """",
							""snippet"": """",
							""additionalProperties"": {
									""document-guid"": ""I111C8210486311E3BB9A89B1597EA15E""

							}
                        }],
         ""SourceDirectoryUrl"" : ""https://(WestlawUKHost)/Search/Results.html?comp=wluk&query=(%Query)&saveJuris=False&contentType=RESEARCH_COMBINED_WLUK&querySubmissionGuid=(NewGuid())"",

}";

		private const string userInfoJson = @"
		{
			""name"": ""Ryan Hunecke"",
			""id"": ""241"",
		}";

		#endregion

		private DataMappingOAuth CreateDataMapping()
        {
			return new DataMappingOAuth(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerOAuth.Object, _mockCosmosDocumentReader.Object, _mockMapper.Object, _mockHttpBodyFactory.Object, _mockHttpResponseFactory.Object, _mockContextAccessor.Object, _mockOAuthApirepository.Object, _fakeOAuthOptions, _mockLegalHomeAccessCheck.Object, _fakeConfiguration);
		}

		[TestMethod]
		public async Task TestMapping()
		{
			// ARRANGE
			string appCode = "TestApp";

			_mockRepository.Setup(x => x.MakeRequest(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>(), It.IsAny<string>())).Returns(Task.FromResult(TwoDocumentsJson));

			ConnectorDocument connectorDocument = new ConnectorDocument()
			{
				Info = new ConnectorInfo()
				{
					SubType = "Child"
				},
				ResultLocation = "data.results",
				Request = new ConnectorRequest()
				{ EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
                {
					Type = ResponseContentType.JSON.ToString()
                }
			};
			connectorDocument.CdmMapping = new CDMMapping()
			{
				structured = new List<CDMElement>() {
							{
								new CDMElement(){  name = "Name", responseElement ="name", type = "string"}
							},
							{
								new CDMElement(){ name="Id", responseElement ="id", type = "string"}
							},
							{
								new CDMElement(){ name="WebLink", responseElement ="", type = "string"}
							},
							{
								new CDMElement(){ name="LastOpened", responseElement ="", type = "string"}
							},
							{
								new CDMElement(){ name="LastUpdated", responseElement ="file_edit_date", type = "string"}
							},
							{
								new CDMElement(){ name="LastModifiedUser", responseElement ="last_user_description", type = "string"}
							},
							{
								new CDMElement(){ name="CreatedDate", responseElement ="file_create_date", type = "string"}
							},
							{
								new CDMElement(){ name="CreatedByUser", responseElement ="author_description", type = "string"}
							},
							{
								new CDMElement(){ name="FileType", responseElement ="type", type = "string"}
							},
							{
								new CDMElement(){ name="FileTypeExtended", responseElement ="type_description", type = "string"}
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
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(Task.FromResult(JToken.Parse(TwoDocumentsJson)));
			mockResponseService.Setup(x => x.MapResponse).Returns(true);
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);

			DataMappingOAuth documentMap = CreateDataMapping();

			// ACT
			dynamic documents = await documentMap.GetAndMapResults<dynamic>(connectorDocument, null, null, null, null);

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
				Info = new ConnectorInfo()
				{
					SubType = "Child"
				},
				Request = new ConnectorRequest()
				{ EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON.ToString()
				}
			};
			connectorDocument.CdmMapping = new CDMMapping()
			{
				structured = new List<CDMElement>() {
							{
								new CDMElement(){ name="Name", responseElement ="name", type = "string"}
							},
							{
								new CDMElement(){ name="Id", responseElement ="id", type = "string"}
							},
							{
								new CDMElement(){ name="WebLink", responseElement ="", type = "string"}
							},
							{
								new CDMElement(){ name="LastOpened", responseElement ="", type = "string"}
							},
							{
								new CDMElement(){ name="LastUpdated", responseElement ="file_edit_date", type = "string"}
							},
							{
								new CDMElement(){ name="LastModifiedUser", responseElement ="last_user_description", type = "string"}
							},
							{
								new CDMElement(){ name="CreatedDate", responseElement ="file_create_date", type = "string"}
							},
							{
								new CDMElement(){ name="CreatedByUser", responseElement ="author_description", type = "string"}
							},
							{
								new CDMElement(){ name="FileType", responseElement ="type", type = "string"}
							},
							{
								new CDMElement(){ name="FileTypeExtended", responseElement ="type_description", type = "string"}
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
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(Task.FromResult(JToken.Parse(TwoDocumentsListJson)));
			mockResponseService.Setup(x => x.MapResponse).Returns(true);
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);

			DataMappingOAuth documentMap = CreateDataMapping();

			// ACT
			dynamic documents = await documentMap.GetAndMapResults<dynamic>(connectorDocument, null, null, null, null);

			// ASSERT
			Assert.IsNotNull(documents);
			Assert.AreEqual(2, documents.Count);
		}

		[TestMethod]
		public async Task TestMappingWithoutMappingResponse()
	{
			// ARRANGE
			string appCode = "TestApp";

			_mockRepository.Setup(x => x.MakeRequest(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>(), It.IsAny<string>())).Returns(Task.FromResult(OneDocumentJson));

			ConnectorDocument connectorDocument = new ConnectorDocument()
			{
				Request = new ConnectorRequest()
				{ EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON.ToString()
				}
			};
			connectorDocument.CdmMapping = new CDMMapping()
			{
				structured = new List<CDMElement>() {
							{
								new CDMElement(){ name="SourceDirectoryUrl", responseElement ="https://(WestlawUKHost)/Search/Results.html?comp=wluk&query=(%Query)&saveJuris=False&contentType=RESEARCH_COMBINED_WLUK&querySubmissionGuid=(NewGuid())", format=null, type = "interpolationString"}
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
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(Task.FromResult(JToken.Parse(OneDocumentJson)));
			mockResponseService.Setup(x => x.MapResponse).Returns(false);
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);
			_fakeConfiguration = Utility.CreateConfiguration("WestlawUKHost", "uk.westlaw.com");

			DataMappingOAuth documentMap = CreateDataMapping();

			// ACT
			JObject documents = await documentMap.GetAndMapResults<JObject>(connectorDocument, null, null, null, null);

			// ASSERT
			Assert.IsNotNull(documents);
			Assert.AreEqual(4, documents.Count);
			Assert.IsTrue(documents["SourceDirectoryUrl"].ToString().Contains(_fakeConfiguration["westlawUKHost"].ToString()));
		}

		[TestMethod]
		public async Task TestMappingWithComplexObjectAndArray()
		{
			// ARRANGE
			string appCode = "TestApp";

			_mockRepository.Setup(x => x.MakeRequest(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>(), It.IsAny<string>())).Returns(Task.FromResult(TwoDocumentsJson));

			ConnectorDocument connectorDocument = new ConnectorDocument()
			{
				Info = new ConnectorInfo()
				{
					SubType = "Child"
				},
				ResultLocation = "data.results",
				Request = new ConnectorRequest()
				{ EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON.ToString()
				}
			};
			connectorDocument.CdmMapping = new CDMMapping()
			{
				structured = new List<CDMElement>() {
							{
								new CDMElement(){ name="Name", responseElement ="name", type = "string"}
							},
							{
								new CDMElement(){ name="Id", responseElement ="id", type = "string"}
							},
							{
								new CDMElement(){ name="WebLink", responseElement ="", type = "string"}
							},
							{
								new CDMElement(){ name="ComplexObjectFieldOne", responseElement ="complexObject.testItem1", type = "string"}
							},
							{
								new CDMElement(){ name="ComplexObjectLevelTwo", responseElement ="complexObject.testItem3.level2", type = "string"}
							},
							{
								new CDMElement(){ name="ArrayValue1", responseElement ="testArray.[name:testIndex2].description", type = "string"}
							},
							{
								new CDMElement(){ name="ArrayValue2", responseElement ="testArray.[description:fakeDesc3].name", type = "string"}
							},
							{
								new CDMElement(){ name="boolean", responseElement ="boolTest", type = DataTypes.Bool}
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
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(Task.FromResult(JToken.Parse(TwoDocumentsJson)));
			mockResponseService.Setup(x => x.MapResponse).Returns(true);
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);

			DataMappingOAuth documentMap = CreateDataMapping();

			// ACT
			dynamic documents = await documentMap.GetAndMapResults<dynamic>(connectorDocument, "fakeTransferToken", null, null, null);

			// ASSERT
			Assert.IsNotNull(documents);
			Assert.AreEqual(2, documents.Count);
			Assert.AreEqual("fakeDescription1", documents[0].ComplexObjectFieldOne.ToString());
			Assert.AreEqual("level 2 variable", documents[0].ComplexObjectLevelTwo.ToString());
			Assert.AreEqual("fakeDesc2", documents[0].ArrayValue1.ToString());
			Assert.AreEqual("testIndex3", documents[0].ArrayValue2.ToString());
			Assert.AreEqual(true, (bool)documents[0].boolean);
		}

        [TestMethod]
        public async Task TestMappingWithNestedArray()
        {
            // ARRANGE
            string appCode = "TestApp";

            _mockRepository.Setup(x => x.MakeRequest(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>(), It.IsAny<string>())).Returns(Task.FromResult(TwoDocumentsJson));

            ConnectorDocument connectorDocument1 = new ConnectorDocument()
            {
				Info = new ConnectorInfo()
				{
					SubType = "Child"
				},
				Id = "1",
                ResultLocation = "data.results",
                Request = new ConnectorRequest()
                { EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON.ToString()
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
				Info = new ConnectorInfo()
				{
					SubType = "Child"
				},
				Id = "2",
                ResultLocation = "testArray",
                Request = new ConnectorRequest()
                { EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON.ToString()
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
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(Task.FromResult(JToken.Parse(TwoDocumentsJson)));
			mockResponseService.Setup(x => x.MapResponse).Returns(true);
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);

			DataMappingOAuth documentMap = CreateDataMapping();

			// ACT
			dynamic documents = await documentMap.GetAndMapResults<dynamic>(connectorDocument1, "fakeTransferToken", null, null, null);

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
				Info = new ConnectorInfo()
				{
					SubType = "Child"
				},
				Request = new ConnectorRequest()
				{ EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON.ToString()
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
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(Task.FromResult(JToken.Parse(TwoDocumentsListJson)));
			mockResponseService.Setup(x => x.MapResponse).Returns(true);
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);
			Mock<HttpContext> mockHttpContext = new Mock<HttpContext>();
			var claims = new ClaimsPrincipal();
			var claimsIdent = new ClaimsIdentity(new List<Claim>() { new Claim("scope", DataConstants.LegalHomeScope) });
			claims.AddIdentity(claimsIdent);
			mockHttpContext.Setup(x => x.User).Returns(claims);
			_mockContextAccessor.Setup(x => x.HttpContext).Returns(mockHttpContext.Object);

			DataMappingOAuth documentMap = CreateDataMapping();

			// ACT
			dynamic documents = await documentMap.GetAndMapResults<dynamic>(connectorDocument, null, null, null, null);

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
				Info = new ConnectorInfo()
				{
					SubType = "Child"
				},
				Request = new ConnectorRequest()
				{ EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON.ToString()
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
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(Task.FromResult(JToken.Parse(TwoDocumentsListJson)));
			mockResponseService.Setup(x => x.MapResponse).Returns(true);
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);

			DataMappingOAuth documentMap = CreateDataMapping();

			// ACT
			dynamic documents = await documentMap.GetAndMapResults<dynamic>(connectorDocument, null, null, null, null);

			// ASSERT
			Assert.IsNotNull(documents);
			Assert.AreEqual(2, documents.Count);
		}

		[TestMethod]
		public async Task VerifyModifyResult()
		{
			// ARRANGE
			string appCode = "TestApp";

			_mockRepository.Setup(x => x.MakeRequest(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>(), It.IsAny<string>())).Returns(Task.FromResult(TwoDocumentsJson));

			ConnectorDocument connectorDocument = new ConnectorDocument()
			{
				Info = new ConnectorInfo()
				{
					SubType = "Child"
				},
				ResultLocation = "data.results",
				Request = new ConnectorRequest()
				{ EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON.ToString()
				}
			};
			connectorDocument.CdmMapping = new CDMMapping()
			{
				structured = new List<CDMElement>() {
							{
								new CDMElement(){ name="Name", responseElement ="name", type = "string"}
							},
							{
								new CDMElement(){ name="Id", responseElement ="id", type = "string"}
							},
							{
								new CDMElement(){ name="url", responseElement ="https://fakeurl.tr.com/(notEncoded)/files?q=(%Query)&t=(doesntExist)", type = DataTypes.InterpolationString}
							},
							{
								new CDMElement(){ name="boolean", responseElement ="boolTest", type = DataTypes.Bool}
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
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(Task.FromResult(JToken.Parse(TwoDocumentsJson)));
			mockResponseService.Setup(x => x.MapResponse).Returns(true);
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);

			DataMappingOAuth documentMap = CreateDataMapping();

			Dictionary<string, string> fakeRequestparams = new Dictionary<string, string>();
			fakeRequestparams.Add("Query", "this is a fake search");
			fakeRequestparams.Add("notEncoded", "not/encoded");

			// ACT
			dynamic documents = await documentMap.GetAndMapResults<dynamic>(connectorDocument, null, null, null, fakeRequestparams);

			// ASSERTAssert.IsNotNull(documents);
			Assert.IsNotNull(documents);
			Assert.AreEqual(2, documents.Count);
			Assert.AreEqual(true, (bool)documents[0].boolean);
			Assert.AreEqual(@"https://fakeurl.tr.com/not/encoded/files?q=this+is+a+fake+search&t=", (string)documents[0].url);
		}

		[TestMethod]
		public async Task VerifyModifyResultForDateTimeFormat()
		{
			// ARRANGE
			string appCode = "TestApp";

			_mockRepository.Setup(x => x.MakeRequest(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>(), It.IsAny<string>())).Returns(Task.FromResult(OneDocumentJson));

			ConnectorDocument connectorDocument = new ConnectorDocument()
			{
				Info = new ConnectorInfo()
				{
					SubType = "Child"
				},
				ResultLocation = "data.results",
				Request = new ConnectorRequest()
				{ EndpointPath = "https://fakeaddress.thomsonreuters.com" },
				Response = new ConnectorResponse()
				{
					Type = ResponseContentType.JSON.ToString()
				}
			};
			connectorDocument.CdmMapping = new CDMMapping()
			{
				structured = new List<CDMElement>() {
							{
								new CDMElement(){ name="Name", responseElement ="name", type = "string"}
							},
							{
								new CDMElement(){ name="Id", responseElement ="id", type = "string"}
							},
							{
								new CDMElement(){ name="url", responseElement ="https://fakeurl.tr.com/(notEncoded)/files?q=(%Query)&t=(doesntExist)", type = DataTypes.InterpolationString}
							},
							{
								new CDMElement(){ name="boolean", responseElement ="boolTest", type = DataTypes.Bool}
							},
							{
								new CDMElement(){ name="creationDate", responseElement ="create_date", type = DataTypes.DateTime, format="yyyyMMdd"}
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
			mockResponseService.Setup(x => x.GetJTokenResponse(It.IsAny<string>(), It.IsAny<ConnectorResponse>())).Returns(Task.FromResult(JToken.Parse(OneDocumentJson)));
			mockResponseService.Setup(x => x.MapResponse).Returns(true);
			_mockHttpResponseFactory.Setup(x => x.GetImplementation(It.IsAny<string>())).Returns(mockResponseService.Object);

			DataMappingOAuth documentMap = CreateDataMapping();

			Dictionary<string, string> fakeRequestparams = new Dictionary<string, string>();
			fakeRequestparams.Add("Query", "this is a fake search");
			fakeRequestparams.Add("notEncoded", "not/encoded");

			// ACT
			dynamic documents = await documentMap.GetAndMapResults<dynamic>(connectorDocument, null, null, null, fakeRequestparams);
			JToken jToken = JToken.FromObject(documents);

			// ASSERT

			Assert.IsNotNull(documents);
			var date = (System.DateTime)jToken.Last["creationDate"];
			Assert.AreEqual(4, date.Month);
			Assert.AreEqual(21, date.Day);
			Assert.AreEqual(2015, date.Year);
			Assert.AreEqual(0, date.Hour);
			Assert.AreEqual(0, date.Minute);
			Assert.AreEqual(0, date.Second);

		}

        [TestMethod]
        public void UpdateOffsetTest()
        {
            // ARRANGE
            string AppCode = string.Empty;
            var availableRegistrations = MockConnectorData.SetupAvailableUserRegistrations().ToList();
            Dictionary<string, string> queryParameters = new Dictionary<string, string>() { { "Offset", "5" }, { "ResultSize", "10" } };
            DataMappingService dataMappingService = new DataMappingService(_cosmosService, _mockMapper.Object, _mockOAuthServices.Object);

            queryParameters = dataMappingService.UpdateOffset(AppCode, availableRegistrations, queryParameters);

			Assert.IsNotNull(queryParameters);
			Assert.AreEqual(queryParameters["ResultSize"], "15");
		}
    }
}
