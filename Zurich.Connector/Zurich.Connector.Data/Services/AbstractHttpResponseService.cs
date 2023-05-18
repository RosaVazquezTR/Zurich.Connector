using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using JUST;
using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Collections.Specialized;
using System.Net.Http;
using Zurich.Common.Repositories;
using Zurich.Common.Services;
using System.Text.RegularExpressions;
using Zurich.Common.Models.IHDocumentStorage;

namespace Zurich.Connector.Data.Services
{
    public abstract class AbstractHttpResponseService
    {
        public virtual bool MapResponse { get; set; }
        public async virtual Task<JToken> GetJTokenResponse(string response, ConnectorResponse connectorResponse)
        {
            return JToken.Parse(response);
        }
        public async virtual Task<JToken> GetJTokenResponse(string response, ConnectorResponse connectorResponse, string connectorId, Dictionary<string, string> requestParameter, NameValueCollection query, IHttpClientFactory httpClientFactory)
        {
            string responseToTransform = response;
            if ((connectorResponse.UsePermissionsCheck ?? false) && (responseToTransform != "[]"))
            {
                // TT DMS permissions check
                JObject response_objects = JObject.Parse("{\"Documents\":" + response + "}");
                List<string> idList = new List<string>();
                foreach (var document in response_objects["Documents"])
                {
                    idList.Add(document["id"].ToString());
                }

                IHDocumentStorageRepository iHDocumentStorageRepository = new IHDocumentStorageRepository(httpClientFactory);
                IHDocumentStorageService iHDocumentStorageService = new IHDocumentStorageService(iHDocumentStorageRepository);
                IHDocumentStorageMappingRequest iHDocumentStorageMappingRequest = new IHDocumentStorageMappingRequest();
                iHDocumentStorageMappingRequest.ids = idList;
                IHDocumentStorageMappingResponse mappings = await iHDocumentStorageService.GetDocumentsMappings("ca353457-26ad-473e-ab46-aae01309a093", iHDocumentStorageMappingRequest);
                List<string> documentIds = mappings.getMappedIds();
                Dictionary<string, string> mappingDictionary = mappings.GetMappingDict();

                IHDocumentStoragePermissionsRequest iHDocumentStoragePermissionsRequest = new IHDocumentStoragePermissionsRequest();
                iHDocumentStoragePermissionsRequest.appCode = "iManageServiceApp";

                Dictionary<string, string> permissionsDictionary = new Dictionary<string, string>();
                IHDocumentStoragePermissionsResponse permissions = new IHDocumentStoragePermissionsResponse();
                if (documentIds.Count > 0)
                {
                    iHDocumentStoragePermissionsRequest.documentIds = documentIds;
                    permissions = await iHDocumentStorageService.GetDocumentsPermissions(iHDocumentStoragePermissionsRequest);
                }
                // If document is not found at user's iManage repo, Permissions endpoint returns a 204 and IHDocumentStoragePermissionsResponse object becomes null.
                if ((permissions != null ) && (permissions.permissions != null ))
                {
                    permissionsDictionary = permissions.GetPermissionsDict();
                }
                JArray filteredDocs = new JArray();
                foreach (var document in response_objects["Documents"])
                {
                    string permission = String.Empty;
                    string capId = document["id"].ToString();
                    if (!String.IsNullOrEmpty(mappingDictionary[capId]) && permissionsDictionary.TryGetValue(mappingDictionary[capId], out permission))
                    {

                        JObject tokenObject = (JObject)document;
                        tokenObject.Add("capabilityDocId", capId);
                        tokenObject.Add("integrationDocId", mappingDictionary[capId]);
                        filteredDocs.Add(tokenObject);
                    }
                }
                responseToTransform = filteredDocs.ToString();
            }

            // TT ClauseBank transformation response
            string provisionID = query["provisionID"];

            JToken targetObject = JArray.Parse(requestParameter["filters"]).FirstOrDefault(x => (string)x["key"] == "clauseTermIDs");
            JToken clauseTerms = targetObject?["value"];

            string[] keyWord = query["keyWord"].Split(",_", StringSplitOptions.RemoveEmptyEntries);
            string input = "{\"Documents\":" + responseToTransform + "}";
            string path = Directory.GetCurrentDirectory() + "\\Transformation\\TTtransformer3.json";
            string transformer = File.ReadAllText(path);
            string transformedString = JsonTransformer.Transform(transformer, input); //Add the thoughtId and thoughtTypeId fields to toug
            JObject jObjectTop = new JObject();
            var obj = JObject.Parse(transformedString);
            JArray acumulate = new JArray();
            List<string> validThoughtIds = new List<string>();

            foreach (var document in obj["Documents"])
            {
                var docId = document["documentId"];
                var capId = document["capabilityDocId"];
                var integrationID = document["integrationDocId"];
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

                            string oldHighlight = field["highlightedText"].Value<string>();
                            string newHighlight = oldHighlight.Replace("<mark>", String.Empty).Replace("</mark>", String.Empty);
                            foreach (string word in keyWord)
                            {
                                if (Regex.Matches(word, @"\b\w+\b").Count > 1)
                                    // if theres more than 1 word in SearchTerm (that implies that is quoted), do not look for string boundaries \b in regex.
                                    newHighlight = Regex.Replace(newHighlight, @"(" + word + @"([^\s]?\w?|\w*))", "<mark>$1</mark>", RegexOptions.IgnoreCase);
                                else
                                    newHighlight = Regex.Replace(newHighlight, @"\b(" + word + @"([^\s]?\w?|\w*))\b", "<mark>$1</mark>", RegexOptions.IgnoreCase);
                            }
                            //If new highlight contains no <mark> tag, return the default highlight provided by ThoughtTrace.
                            if (newHighlight.Contains("<mark>"))
                                field["highlightedText"] = newHighlight;
                            else
                                field["highlightedText"] = oldHighlight;
                            //If there are capabilityDocID and InternalDocID, add them
                            if (!String.IsNullOrEmpty(capId.ToString()) && !String.IsNullOrEmpty(integrationID.ToString()))
                            {
                                field["capabilityDocId"] = capId;
                                field["integrationDocId"] = integrationID;
                            }

                            acumulate.Add(field);
                        }
                        else if (clauseTerms.Any(x => x.Value<string>() == field["clauseTermId"].ToString()))
                        {
                            validThoughtIds.Add(id);
                        }
                    }
                }
            }
            //Direct damage cap condition. If clauseterm is selected, it only returns the Provision fields that comes together a clauseTerm field. (See US #174875)
            if (validThoughtIds.Any())
            {
                acumulate = new JArray(acumulate.Where(x => validThoughtIds.Contains(x["thoughtId"].Value<string>())));
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

            paginationDoc.from = (currentPage - 1) * resultsPerPage + 1;
            paginationDoc.to = ((currentPage) * resultsPerPage > totalThoughts) ? totalThoughts : (currentPage) * resultsPerPage;
            paginationDoc.of = totalThoughts;

            paginationDoc.PagesPaginationInfo = new JObject();
            paginationDoc.PagesPaginationInfo.current = currentPage;
            paginationDoc.PagesPaginationInfo.totalPages = totalPages;
            int[] display = Enumerable.Range(1, totalPages).ToArray();
            paginationDoc.PagesPaginationInfo.display = new JArray(display);

            if (currentPage + 1 > totalPages || totalPages == 1)
                paginationDoc.PagesPaginationInfo.next = null;
            else
                paginationDoc.PagesPaginationInfo.next = currentPage + 1;

            if (currentPage - 1 == 0)
                paginationDoc.PagesPaginationInfo.previous = null;
            else
                paginationDoc.PagesPaginationInfo.previous = currentPage - 1;

            if (totalThoughts > 0)
                jObjectTop.Add(new JProperty("pagination", paginationDoc));
            jObjectTop.Add(new JProperty("totalDocs", totalThoughts));

            return jObjectTop;
        }

    }
}
