using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.Services
{
    public class HttpXmlResponseService : AbstractHttpResponseService, IHttpResponseService
    {
        public HttpXmlResponseService()
        {
            MapResponse = true;
        }

        public async override Task<JToken> GetJTokenResponse(string response, ConnectorResponse connectorResponse)
        {

            XmlDocument xmlDoc = new XmlDocument();
            string jsonText = string.Empty;

            xmlDoc.LoadXml(response);
            if (connectorResponse.XmlArrayAttribute.Any())
            {
                foreach (var arrayElement in connectorResponse.XmlArrayAttribute)
                {
                    // if the search result has only one xml record, manually convert that into an array.
                    xmlDoc = AddJsonArrayAttributes(xmlDoc, arrayElement);

                }
            }
            jsonText = JsonConvert.SerializeXmlNode(xmlDoc);

            return JToken.Parse(jsonText);
        }

        /// <summary>
		/// Convert any single element XML to a JSON Array by adding a tag "json:Array='true'".
		/// TODO - Replace this with any better solution.
		/// </summary>
		/// <param name="xmlArrayAttribute"></param>
		/// <param name="doc"></param>
		private XmlDocument AddJsonArrayAttributes(XmlDocument doc, string xmlArrayAttribute)
        {
            var elements = doc.SelectNodes(xmlArrayAttribute);
            if (elements != null)
            {
                foreach (var element in elements)
                {
                    // Below namespaceURL is required to work the functionality.
                    var jsonArray = doc.CreateAttribute("json", "Array", "http://james.newtonking.com/projects/json");
                    jsonArray.Value = "true";
                    (element as XmlElement).SetAttributeNode(jsonArray);
                }
            }
            return doc;
        }

    }
}
