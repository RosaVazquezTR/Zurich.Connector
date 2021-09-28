using Zurich.Connector.Data.Repositories.CosmosDocuments;

namespace Zurich.Connector.Data.Model
{
    public class PostRequestParameter : ConnectorRequestParameter
    {
        public PostRequestParameter(ConnectorRequestParameter connectorParameter)
        {
            Name = connectorParameter.Name;
            InClause = connectorParameter.InClause;
            Required = connectorParameter.Required;
            Type = connectorParameter.Type;
            DefaultValue = connectorParameter.DefaultValue;
            Cdmname = connectorParameter.Cdmname;
            Tags = connectorParameter.Tags;
    }

        public string ParamValue { get; set; }
    }
}
