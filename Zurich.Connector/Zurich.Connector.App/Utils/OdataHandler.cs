using System;
using System.Collections.Generic;
using System.Linq;
using Zurich.Connector.App.Model;

namespace Zurich.Connector.App.Utils
{
    public static class ODataHandler
    {
		public static Dictionary<string, string> BuildQueryParams(Dictionary<string, string> queryParameters, ConnectorModel connectorModel)
		{
			List<ConnectorRequestParameterModel> odataParams = connectorModel?.Request?.Parameters.FindAll(param => param.InClause == ODataConstants.OData);
			Dictionary<string, string> odataQuery = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			foreach (var param in odataParams)
            {
				switch (param.Name)
                {
					case ODataConstants.Filter:
						string filterQuery = GetQueryStringFromQueryOrDefault(queryParameters, param);

						if (!string.IsNullOrEmpty(filterQuery))
                        {
							var terms = filterQuery.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

							// TODO: Will need to find a new solution for handling 'eq', 'or' and other comparisons in the future
							var expressions = terms.Select(filter => $"{param.Key} eq '{filter}'");
							var finalOdataQuery = string.Join(" or ", expressions);

							if (odataQuery.ContainsKey(param.Name))
								odataQuery[param.Name] = $"{odataQuery[param.Name]} and ({finalOdataQuery})";
							else
								odataQuery.Add(param.Name, $"({finalOdataQuery})");
						}
						break;
					default:
						filterQuery = GetQueryStringFromQueryOrDefault(queryParameters, param);

						if (!string.IsNullOrEmpty(filterQuery))
							odataQuery.Add(param.Name, filterQuery);
						break;
				}
            }

			return odataQuery;
		}

		public static bool HasODataParams(ConnectorModel connectorModel)
        {
			return connectorModel?.Request?.Parameters?.Exists(param => param.InClause == ODataConstants.OData) ?? false;
		}

		private static string GetQueryStringFromQueryOrDefault(Dictionary<string, string> queryParameters, ConnectorRequestParameterModel param)
        {
			if (queryParameters.TryGetValue(param.CdmName, out string filters))
				return filters;
			else if (!string.IsNullOrEmpty(param.DefaultValue))
				return param.DefaultValue;

			return null;
		}
	}


}
