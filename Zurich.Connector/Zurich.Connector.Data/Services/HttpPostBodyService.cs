using Newtonsoft.Json.Linq;
using System;
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
				// remove so it doesn't get stuck on the query string.
				parameters.Remove(parameter.Name);
			}
			return JsonRequest.ToString(Newtonsoft.Json.Formatting.None);
		}

		private JTokenWriter SetupPostJWriter(PostRequestParameter param)
		{
			JTokenWriter writer = new JTokenWriter();
			var parts = param.Name.Split('.');
			WriteJsonObject(writer, parts, param.ParamValue, param.Type);
			return writer;
		}

		private void WriteJsonObject(JTokenWriter writer, string[] paramParts, string value, string valueType)
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
				WriteJsonObject(writer, paramParts.Skip(1).ToArray(), value, valueType);
			else
			{
				var requestArray = value?.Split(',', StringSplitOptions.RemoveEmptyEntries);
				if (requestArray != null)
				{
					foreach (var requestValue in requestArray)
					{
						WriteTypedValue(writer, requestValue, valueType);
					}
				}
			}

			if (isArray)
				writer.WriteEndArray();
			else
				writer.WriteEndObject();
		}

		/// <summary>
		/// Writes a value to a JSON property taking its type into consideration
		/// </summary>
		/// <param name="writer">The <see cref="JTokenWriter"/> to use</param>
		/// <param name="value">The value</param>
		/// <param name="valueType">The value type</param>
		private void WriteTypedValue(JTokenWriter writer, string value, string valueType)
        {
			switch(valueType)
            {
                case "int":
					var intValue = int.Parse(value);
					writer.WriteValue(intValue);
                    break;
				case "bool":
					var boolValue = bool.Parse(value);
					writer.WriteValue(boolValue);
					break;
				case "double":
					var doubleValue = double.Parse(value);
					writer.WriteValue(doubleValue);
					break;
				default:
					writer.WriteValue(value);
					break;
            }
        }
	}
}
