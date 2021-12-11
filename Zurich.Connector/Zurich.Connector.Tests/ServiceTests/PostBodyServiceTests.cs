using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Collections.Specialized;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Data.Services;

namespace Zurich.Connector.Tests
{

    [TestClass]
    public class PostBodyServiceTests
    {

        [TestInitialize]
        public void TestInitialize()
        {
        }

        [TestMethod]
        public void TestMapping()
        {
            // ARRANGE
            HttpPostBodyService postBodyService = new HttpPostBodyService();

            ConnectorDocument document = new ConnectorDocument()
            {
                Request = new ConnectorRequest()
                {
                    Parameters = new List<ConnectorRequestParameter>()
                    {
                        new ConnectorRequestParameter() {
                            CdmName = "Query",
                            Name = "requests.[].query.queryString",
                            InClause = "body"
                        },
                        new ConnectorRequestParameter() {
                            CdmName = "EntityType",
                            Name = "requests.[].entityTypes.[]",
                            InClause = "body"
                        }
                    }
                }
            };

            NameValueCollection collection = new NameValueCollection();
            collection["requests.[].entityTypes.[]"] = "driveItems";
            collection["requests.[].query.queryString"] = "docs";

            // ACT
            var postBody = postBodyService.CreateBody(document, collection);

            // ASSERT
            Assert.AreEqual("{\"requests\":[{\"query\":{\"queryString\":\"docs\"},\"entityTypes\":[\"driveItems\"]}]}", postBody);
        }

        [TestMethod]
        public void TestMappingMultiParam()
        {
            // ARRANGE
            HttpPostBodyService postBodyService = new HttpPostBodyService();

            ConnectorDocument document = new ConnectorDocument()
            {
                Request = new ConnectorRequest()
                {
                    Parameters = new List<ConnectorRequestParameter>()
                    {
                        new ConnectorRequestParameter() {
                            CdmName = "Query",
                            Name = "requests.[].query.queryString",
                            InClause = "body"
                        },
                        new ConnectorRequestParameter() {
                            CdmName = "EntityType",
                            Name = "requests.[].entityTypes.[]",
                            InClause = "body"
                        },
                        new ConnectorRequestParameter() {
                            CdmName = "Fields",
                            Name = "requests.[].fields.[]",
                            InClause = "body"
                        }
                    }
                }
            };

            NameValueCollection collection = new NameValueCollection();
            collection["requests.[].entityTypes.[]"] = "driveItems";
            collection["requests.[].query.queryString"] = "docs";
            collection["requests.[].fields.[]"] = "Name,CreatedDateTime,LastModifiedDateTime,LastModifiedBy,WebUrl,ParentReference";

            // ACT
            var postBody = postBodyService.CreateBody(document, collection);

            // ASSERT
            Assert.AreEqual("{\"requests\":[{\"query\":{\"queryString\":\"docs\"},\"entityTypes\":[\"driveItems\"],\"fields\":[\"Name\",\"CreatedDateTime\",\"LastModifiedDateTime\",\"LastModifiedBy\",\"WebUrl\",\"ParentReference\"]}]}", postBody);
        }

        [TestMethod]
        public void TestMappingNullValues()
        {
            // ARRANGE
            HttpPostBodyService postBodyService = new HttpPostBodyService();

            ConnectorDocument document = new ConnectorDocument()
            {
                Request = new ConnectorRequest()
                {
                    Parameters = new List<ConnectorRequestParameter>()
                    {
                        new ConnectorRequestParameter() {
                            CdmName = "Query",
                            Name = "requests.[].query.queryString",
                            InClause = "body"
                        },
                        new ConnectorRequestParameter() {
                            CdmName = "EntityType",
                            Name = "requests.[].entityTypes.[]",
                            InClause = "body"
                        },
                        new ConnectorRequestParameter() {
                            CdmName = "Fields",
                            Name = "requests.[].fields.[]",
                            InClause = "body"
                        }
                    }
                }
            };

            NameValueCollection collection = new NameValueCollection();
            collection["requests.[].entityTypes.[]"] = "driveItems";
            collection["requests.[].query.queryString"] = "docs";
            collection["requests.[].fields.[]"] = null;

            // ACT
            var postBody = postBodyService.CreateBody(document, collection);

            // ASSERT
            Assert.AreEqual("{\"requests\":[{\"query\":{\"queryString\":\"docs\"},\"entityTypes\":[\"driveItems\"],\"fields\":[]}]}", postBody);
        }

    }
}
