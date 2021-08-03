using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.Model
{
    public class PostRequestParameter : ConnectorRequestParameter
    {
        public PostRequestParameter(ConnectorRequestParameter connectorParameter)
        {
            name = connectorParameter.name;
            inClause = connectorParameter.inClause;
            required = connectorParameter.required;
            type = connectorParameter.type;
            defaultValue = connectorParameter.defaultValue;
            cdmname = connectorParameter.cdmname;
            tags = connectorParameter.tags;
    }

        public string ParamValue { get; set; }
    }
}
