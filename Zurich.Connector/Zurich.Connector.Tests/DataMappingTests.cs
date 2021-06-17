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

		[TestInitialize]
		public void TestInitialize()
		{
			_mockRepository = new Mock<IRepository>();
			_mockDataMappingRepository = new Mock<IDataMappingRepository>();
			_mockOAuthService = new Mock<IOAuthService>();
			_mockLoggerOAuth = new Mock<ILogger<DataMappingOAuth>>();
			_mockLoggerTransfer = new Mock<ILogger<DataMappingTransfer>>();

			// feels like this won't change
			AppToken token = new AppToken() { access_token = "fakeToken" };
			_mockOAuthService.Setup(x => x.GetToken(It.IsAny<string>(), It.IsAny<OAuthApplicationType>(), It.IsAny<bool>(), It.IsAny<string>(), It.IsAny<ProductType>())).Returns(Task.FromResult(token));

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

			_mockRepository.Setup(x => x.Get(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>())).Returns(Task.FromResult(TwoDocumentsJson));
			DataMappingClass dataMap = new DataMappingClass()
			{
				Api = new DataMappingApiRequest() { Url = "https://fakeaddress.thomsonreuters.com", AuthHeader = "differentAuthHeader" },
				AppCode = appCode,
				ResultLocation = "data.results",
				Mapping = new List<DataMappingProperty>() {
					new DataMappingProperty(){CDMProperty = "Name", APIProperty =  "name"},
					new DataMappingProperty(){CDMProperty = "Id", APIProperty =  "id"},
					new DataMappingProperty(){CDMProperty = "WebLink", APIProperty =  ""},
					new DataMappingProperty(){CDMProperty = "LastOpened", APIProperty =  ""},
					new DataMappingProperty(){CDMProperty = "LastUpdated", APIProperty =  "file_edit_date"},
					new DataMappingProperty(){CDMProperty = "LastModifiedUser", APIProperty =  "last_user_description"},
					new DataMappingProperty(){CDMProperty = "CreatedDate", APIProperty =  "file_create_date"},
					new DataMappingProperty(){CDMProperty = "CreatedByUser", APIProperty =  "author_description"},
					new DataMappingProperty(){CDMProperty = "FileType", APIProperty =  "type"},
					new DataMappingProperty(){CDMProperty = "FileTypeExtended", APIProperty =  "type_description"}
				}
			};
			_mockDataMappingRepository.Setup(x => x.GetMap(It.IsAny<string>())).Returns(Task.FromResult(dataMap));

			DataMappingOAuth documentMap = new DataMappingOAuth(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerOAuth.Object);
			
			// ACT
			dynamic documents = await documentMap.Get<dynamic>(dataMap, null);
			
			// ASSERT
			Assert.IsNotNull(documents);
			Assert.AreEqual(2, documents.Count);
		}

		[TestMethod]
		public async Task TestMappingNoresultLocation()
		{
			// ARRANGE
			string appCode = "TestApp";

			_mockRepository.Setup(x => x.Get(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>())).Returns(Task.FromResult(TwoDocumentsListJson));
			DataMappingClass dataMap = new DataMappingClass()
			{
				Api = new DataMappingApiRequest() { Url = "https://fakeaddress.thomsonreuters.com", AuthHeader = "differentAuthHeader" },
				AppCode = appCode,
				Mapping = new List<DataMappingProperty>() {
					new DataMappingProperty(){CDMProperty = "Name", APIProperty =  "name"},
					new DataMappingProperty(){CDMProperty = "Id", APIProperty =  "id"},
					new DataMappingProperty(){CDMProperty = "WebLink", APIProperty =  ""},
					new DataMappingProperty(){CDMProperty = "LastOpened", APIProperty =  ""},
					new DataMappingProperty(){CDMProperty = "LastUpdated", APIProperty =  "file_edit_date"},
					new DataMappingProperty(){CDMProperty = "LastModifiedUser", APIProperty =  "last_user_description"},
					new DataMappingProperty(){CDMProperty = "CreatedDate", APIProperty =  "file_create_date"},
					new DataMappingProperty(){CDMProperty = "CreatedByUser", APIProperty =  "author_description"},
					new DataMappingProperty(){CDMProperty = "FileType", APIProperty =  "type"},
					new DataMappingProperty(){CDMProperty = "FileTypeExtended", APIProperty =  "type_description"}
				}
			};
			_mockDataMappingRepository.Setup(x => x.GetMap(It.IsAny<string>())).Returns(Task.FromResult(dataMap));

			DataMappingOAuth documentMap = new DataMappingOAuth(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerOAuth.Object);

			// ACT
			dynamic documents = await documentMap.Get<dynamic>(dataMap);
			
			// ASSERT
			Assert.IsNotNull(documents);
			Assert.AreEqual(2, documents.Count);
		}

		[TestMethod]
		public async Task TestMappingWithComplexObjectAndArray()
		{
			// ARRANGE
			string appCode = "TestApp";

			_mockRepository.Setup(x => x.Get(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>())).Returns(Task.FromResult(TwoDocumentsJson));
			DataMappingClass dataMap = new DataMappingClass()
			{
				Api = new DataMappingApiRequest() { Url = "https://fakeaddress.thomsonreuters.com", AuthHeader = "differentAuthHeader" },
				AppCode = appCode,
				ResultLocation = "data.results",
				Mapping = new List<DataMappingProperty>() {
					new DataMappingProperty(){CDMProperty = "Name", APIProperty =  "name"},
					new DataMappingProperty(){CDMProperty = "Id", APIProperty =  "id"},
					new DataMappingProperty(){CDMProperty = "WebLink", APIProperty =  ""},
					new DataMappingProperty(){CDMProperty = "ComplexObjectFieldOne", APIProperty =  "complexObject.testItem1"},
					new DataMappingProperty(){CDMProperty = "ComplexObjectLevelTwo", APIProperty =  "complexObject.testItem3.level2"},
					new DataMappingProperty(){CDMProperty = "ArrayValue1", APIProperty =  "testArray.[name:testIndex2].description"},
					new DataMappingProperty(){CDMProperty = "ArrayValue2", APIProperty =  "testArray.[description:fakeDesc3].name"},
				}
			};
			_mockDataMappingRepository.Setup(x => x.GetMap(It.IsAny<string>())).Returns(Task.FromResult(dataMap));

			DataMappingTransfer documentMap = new DataMappingTransfer(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerTransfer.Object);

			// ACT
			dynamic documents = await documentMap.Get<dynamic>(dataMap, "fakeTransferToken");
			
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

			_mockRepository.Setup(x => x.Get(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>())).Returns(Task.FromResult(TwoDocumentsJson));
			DataMappingClass dataMap = new DataMappingClass()
			{
				Id = "1",
				Api = new DataMappingApiRequest() { Url = "https://fakeaddress.thomsonreuters.com", AuthHeader = "differentAuthHeader" },
				AppCode = appCode,
				ResultLocation = "data.results",
				Mapping = new List<DataMappingProperty>() {
					new DataMappingProperty(){CDMProperty = "Name", APIProperty =  "name"},
					new DataMappingProperty(){CDMProperty = "Id", APIProperty =  "id"},
					new DataMappingProperty(){CDMProperty = "WebLink", APIProperty =  ""},
					new DataMappingProperty(){CDMProperty = "ObjectArray", APIProperty =  "{2}"},
				}
			};

			// This contains the mapping for the nested array "testArray"
			DataMappingClass dataMap2 = new DataMappingClass()
			{
				Id = "2",
				Api = new DataMappingApiRequest() { Url = "https://fakeaddress.thomsonreuters.com", AuthHeader = "differentAuthHeader" },
				AppCode = appCode,
				ResultLocation = "testArray",
				Mapping = new List<DataMappingProperty>() {
					new DataMappingProperty(){CDMProperty = "Name", APIProperty =  "name"},
					new DataMappingProperty(){CDMProperty = "DescriptionProp", APIProperty =  "description"},
				}
			};
			_mockDataMappingRepository.Setup(x => x.GetMap(It.IsAny<string>())).Returns<string>(id =>
			{
				if (id == "1")
					return Task.FromResult(dataMap);
				else
					return Task.FromResult(dataMap2);
			});

			DataMappingTransfer documentMap = new DataMappingTransfer(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerTransfer.Object);

			// ACT
			dynamic documents = await documentMap.Get<dynamic>(dataMap, "fakeTransferToken");

			// ASSERT
			Assert.IsNotNull(documents);
			Assert.AreEqual(2, documents.Count);
			Assert.AreEqual(3, documents[0].ObjectArray.Count);
			Assert.AreEqual("testIndex2", documents[0].ObjectArray[1].Name.ToString());
            Assert.AreEqual("fakeDesc2", documents[0].ObjectArray[1].DescriptionProp.ToString());
        }


		[TestMethod]
		public async Task VerifyURLFormat()
		{
			// ARRANGE
			string appCode = "TestApp";

			_mockRepository.Setup(x => x.Get(It.IsAny<ApiInformation>(), It.IsAny<NameValueCollection>())).Returns(Task.FromResult(userInfoJson));
			DataMappingClass dataMap = new DataMappingClass()
			{
				Api = new DataMappingApiRequest() { Url = "https://fakeaddress.thomsonreuters.com", AuthHeader = "differentAuthHeader" },
				AppCode = appCode,
				Mapping = new List<DataMappingProperty>() {
					new DataMappingProperty(){CDMProperty = "Name", APIProperty =  "name"},
					new DataMappingProperty(){CDMProperty = "Id", APIProperty =  "id"},
				}
			};
			_mockDataMappingRepository.Setup(x => x.GetMap(It.IsAny<string>())).Returns(Task.FromResult(dataMap));

			DataMappingOAuth documentMap = new DataMappingOAuth(_mockRepository.Object, _mockDataMappingRepository.Object, _mockOAuthService.Object, _mockLoggerOAuth.Object);
			
			// ACT
			string newUrl = await documentMap.UpdateUrl("/work/api/v2/customers/{UserInfo.id}/documents", dataMap);
			
			// ASSERT
			Assert.IsNotNull(newUrl);
			Assert.AreEqual("/work/api/v2/customers/241/documents", newUrl);
		}

	}
}
