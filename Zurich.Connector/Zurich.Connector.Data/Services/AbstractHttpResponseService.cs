﻿using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using JUST;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections.Specialized;
namespace Zurich.Connector.Data.Services
{
    public abstract class AbstractHttpResponseService
    {
        public virtual bool MapResponse { get; set; }
        public async virtual Task<JToken> GetJTokenResponse(string response, ConnectorResponse connectorResponse)
        {
            return JToken.Parse(response);
        }
        public async virtual Task<JToken> GetJTokenResponse(string response, ConnectorResponse connectorResponse, string connectorId, Dictionary<string, string> requestParameter, NameValueCollection query)
        {
            //TODO: add a data source parameter to avoid using connector ids directly in the code
            if (connectorId != "52")
                return JToken.Parse(response);
            else
            // TT transformation response
            {
                string provisionID = query["provisionID"];
                string input = "{\"Documents\":" + response + "}";
                string path = Directory.GetCurrentDirectory() + "\\Transformation\\TTtransformer3.json";
                string transformer = File.ReadAllText(path);
                string transformedString = JsonTransformer.Transform(transformer, input); //Add the thoughtId and thoughtTypeId fields to toug
                JObject jObjectTop = new JObject();
                var obj = JObject.Parse(transformedString);
                JArray acumulate = new JArray();

                foreach (var document in obj["Documents"])
                {
                    var docId = document["documentId"];
                    var name = document["name"];
                    var createdOn = document["createdOn"];
                    var documentTypeId = document["documentTypeId"];
                    var pageCount = document["pageCount"];
                    var lastModifiedOn = document["lastModifiedOn"];
                    var processingStatus = document["processingStatus"];
                    foreach (var thought in document["thoughts"])
                    {
                        var id = thought["id"].Value<string>();
                        var clauseTypeId = thought["clauseTypeId"].Value<string>();
                        foreach (var field in thought["fields"])
                        {
                            if (field["clauseTermId"].ToString() == provisionID)
                            {
                                field["documentId"] = docId;
                                field["name"] = name;
                                field["createdOn"] = createdOn;
                                field["documentTypeId"] = documentTypeId;
                                field["pageCount"] = pageCount;
                                field["lastModifiedOn"] = lastModifiedOn;
                                field["processingStatus"] = processingStatus;
                                field["thoughtId"] = id;
                                field["clauseTypeId"] = clauseTypeId;
                                acumulate.Add(field);
                            }
                        }
                    }
                }

                JProperty documents = new JProperty("Documents", acumulate);
                jObjectTop.Add(documents);


                // Calculating pagination data
                // TT Pagination values
                int currentPage = 1;
                int totalPages = 1;
                int totalThoughts = acumulate.Count();
                int resultsPerPage = totalThoughts;
                dynamic paginationDoc = new JObject();

                if (requestParameter.ContainsKey("resultsPerPage") && Convert.ToInt32(requestParameter["resultsPerPage"]) > 0)
                {
                    resultsPerPage = Convert.ToInt32(requestParameter["resultsPerPage"]);
                    totalPages = (int)Math.Ceiling((decimal)totalThoughts / (decimal)resultsPerPage);
                }
                if (requestParameter.ContainsKey("start") && (Convert.ToInt32(requestParameter["start"]) > 0))
                {
                    currentPage = Convert.ToInt32(requestParameter["start"]);
                    if (currentPage > totalPages) 
                    {
                        currentPage = totalPages;
                    }
                }
                
                paginationDoc.from = (currentPage-1) * resultsPerPage+1;
                paginationDoc.to = ((currentPage)*resultsPerPage> totalThoughts)? totalThoughts : (currentPage) * resultsPerPage;
                paginationDoc.of = totalThoughts;

                paginationDoc.PagesPaginationInfo = new JObject();
                paginationDoc.PagesPaginationInfo.current = currentPage;
                paginationDoc.PagesPaginationInfo.totalPages = (int)totalPages;
                int[] display = Enumerable.Range(1, (int)totalPages).ToArray();
                paginationDoc.PagesPaginationInfo.display = new JArray(display);

                if (currentPage + 1 > totalPages || totalPages == 1)
                    paginationDoc.PagesPaginationInfo.next = null;
                else
                    paginationDoc.PagesPaginationInfo.next = currentPage + 1;

                if (currentPage - 1 == 0)
                    paginationDoc.PagesPaginationInfo.previous = null;
                else
                    paginationDoc.PagesPaginationInfo.previous = currentPage - 1;

                if(totalThoughts>0)
                    jObjectTop.Add(new JProperty("pagination", paginationDoc));
                jObjectTop.Add(new JProperty("totalDocs", totalThoughts));

                return jObjectTop;
            }
        }

    }
}
