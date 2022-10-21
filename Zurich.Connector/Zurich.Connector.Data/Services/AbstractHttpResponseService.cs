using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using JUST;
using System.IO;

namespace Zurich.Connector.Data.Services
{
    public abstract class AbstractHttpResponseService
	{
		public virtual bool MapResponse { get; set; }
		public async virtual Task<JToken> GetJTokenResponse(string response, ConnectorResponse connectorResponse)
		{
			return JToken.Parse(response);
		}
		public async virtual Task<JToken> GetJTokenResponse(string response, ConnectorResponse connectorResponse, string connectorId)
		{
			if (connectorId != "52")
				return JToken.Parse(response);
            else
			// TT transformation response
            {
                string input = "{\"Documents\":" + response + "}";
                string path = Directory.GetCurrentDirectory()+"\\Transformation\\TTtransformer3.json";
                string transformer = File.ReadAllText(path);
                string transformedString = JsonTransformer.Transform(transformer, input); //Add the thoughtId and thoughtTypeId fields to toug

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
                        var thoughtTypeId = thought["thoughtTypeId"].Value<string>();
                        foreach (var field in thought["fields"])
                        {
                            field["documentId"] = docId;
                            field["name"] = name;
                            field["createdOn"] = createdOn;
                            field["documentTypeId"] = documentTypeId;
                            field["pageCount"] = pageCount;
                            field["lastModifiedOn"] = lastModifiedOn;
                            field["processingStatus"] = processingStatus;
                            field["thoughtId"] = id;
                            field["thoughtTypeId"] = thoughtTypeId;
                            acumulate.Add(field);
                        }
                    }
                }
                obj["documents"] = acumulate;
                string showMap = obj["documents"].ToString();
                return JToken.Parse(obj["documents"].ToString()); 

            }
        }

	}
}
