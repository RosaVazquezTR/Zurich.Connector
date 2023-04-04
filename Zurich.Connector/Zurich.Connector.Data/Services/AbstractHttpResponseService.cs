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
            // TT transformation response
            string provisionID = query["provisionID"];
            string[] keyWord = query["keyWord"].Split(",_", StringSplitOptions.RemoveEmptyEntries);
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

                            string newHighlight = field["highlightedText"].Value<string>().Replace("<mark>", String.Empty).Replace("</mark>", String.Empty);
                            foreach (string word in keyWord)
                            {
                                if (Regex.Matches(word, @"\b\w+\b").Count > 1)
                                    // if theres more than 1 word in SearchTerm (that implies that is quoted), do not look for string boundaries \b in regex.
                                    newHighlight = Regex.Replace(newHighlight, @"(" + word + @"([^\s]?\w?|\w*))", "<mark>$1</mark>", RegexOptions.IgnoreCase);
                                else
                                    newHighlight = Regex.Replace(newHighlight, @"\b(" + word + @"([^\s]?\w?|\w*))\b", "<mark>$1</mark>", RegexOptions.IgnoreCase);
                            }
                            field["highlightedText"] = newHighlight;

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

            paginationDoc.from = (currentPage - 1) * resultsPerPage + 1;
            paginationDoc.to = ((currentPage) * resultsPerPage > totalThoughts) ? totalThoughts : (currentPage) * resultsPerPage;
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

            if (totalThoughts > 0)
                jObjectTop.Add(new JProperty("pagination", paginationDoc));
            jObjectTop.Add(new JProperty("totalDocs", totalThoughts));

            return jObjectTop;
        }
    }
}
