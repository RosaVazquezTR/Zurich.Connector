using Newtonsoft.Json.Linq;
using System.Collections.Specialized;
using System.Linq;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.Services
{

    public class HttpPostBodyService: AbstractHttpBodyService, IHttpBodyService
	{
		public HttpPostBodyService()
		{
		}

		public override string CreateBody(ConnectorDocument connectorDocument, NameValueCollection parameters)
		{
			var requestMappingParameters = connectorDocument?.Request?.Parameters;
			if (requestMappingParameters == null)
			{
				return null;
			}

			JContainer JsonRequest = new JObject();
			var settings = new JsonMergeSettings { MergeArrayHandling = MergeArrayHandling.Merge };

			var neededParams = requestMappingParameters.Where(x => parameters.AllKeys.Contains(x.Name)).Select(y => new PostRequestParameter(y) { ParamValue = parameters[y.Name] });
			foreach (var parameter in neededParams)
			{
				JTokenWriter writer = SetupPostJWriter(parameter);
				JsonRequest.Merge(writer.Token, settings);
			}
			return JsonRequest.ToString(Newtonsoft.Json.Formatting.None);
		}

		private JTokenWriter SetupPostJWriter(PostRequestParameter param)
		{
			JTokenWriter writer = new JTokenWriter();
			var parts = param.Name.Split('.');
			WriteJsonObject(writer, parts, param.ParamValue);
			return writer;
		}

		private void WriteJsonObject(JTokenWriter writer, string[] paramParts, string value)
		{
			var paramPart = paramParts.First();
			var isArray = paramPart == "[]";

			if (isArray)
			{
				writer.WriteStartArray();
			}
			else
			{
				writer.WriteStartObject();
				writer.WritePropertyName(paramPart);
			}

			if (paramParts.Count() != 1)
				WriteJsonObject(writer, paramParts.Skip(1).ToArray(), value);
			else
			{
				var requestArray = value?.Split(',');
				if (requestArray != null)
				{
					foreach (var requestValue in requestArray)
					{
						writer.WriteValue(requestValue);
					}
				}
			}

			if (isArray)
				writer.WriteEndArray();
			else
				writer.WriteEndObject();
		}

	}
}
