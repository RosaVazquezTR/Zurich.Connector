using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Xsl;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using System.Reflection;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.Data.Services
{
    public class HttpXslResponseService : AbstractHttpResponseService, IHttpResponseService
    {

        protected ConnectorCosmosContext _cosmosContext;
        protected IMapper _mapper;
        protected IConfiguration _configuration;

        public HttpXslResponseService(ConnectorCosmosContext cosmosContext, IMapper mapper, IConfiguration configuration)
        {
            _cosmosContext = cosmosContext;
            _mapper = mapper;
            MapResponse = false;
        }

        public async override Task<JToken> GetJTokenResponse(string response, ConnectorResponse connectorResponse)
        {

            TransformDocument transform = await GetTransform(connectorResponse.TransformationLocation);

            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(response);

            XslCompiledTransform objXslTrans = new XslCompiledTransform();
            objXslTrans.Load(new XmlTextReader(new StringReader(transform.transform)));

            StringWriter stringWriter = new StringWriter();
            XmlTextWriter xmlTextWriter = new XmlTextWriter(stringWriter);
            xmlTextWriter.Formatting = System.Xml.Formatting.Indented;
            objXslTrans.Transform(xmlDocument, null, xmlTextWriter);
            xmlTextWriter.Flush();
            response = await FormatResponse(stringWriter.ToString());

            string jsonText = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(response));
            return JToken.Parse(jsonText);
        }


        /// <summary>
        /// Format connector response
        /// </summary>
        /// <returns>data source.</returns> 
        public async Task<string> FormatResponse(string response)
        {
            try
            {
                SearchResponse searchObject = JsonConvert.DeserializeObject<SearchResponse>(response);
                if (searchObject.Documents != null)
                {
                    foreach (SearchItemEntity item in searchObject.Documents)
                    {
                        item.Title = item.Title.Replace("\"", string.Empty);
                        item.Snippet = item.Snippet.Replace("\"", string.Empty);

                    }
                }
            }
            catch(Exception ex)
            {
                response = string.Empty;
            }
            response = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(response));
            return response;
        }


        /// <summary>
        /// Fetch a data sources from Cosmos by dataSourceID
        /// </summary>
        /// <returns>data source.</returns> 
        public async Task<TransformDocument> GetTransform(string transformId)
        {
            var transformDocument = await _cosmosContext.GetDocument<TransformDocument>
                                        (CosmosConstants.TransformContainerId, transformId, CosmosConstants.TransformPartitionKey);
            return _mapper.Map<TransformDocument>(transformDocument);
        }
    }
}
