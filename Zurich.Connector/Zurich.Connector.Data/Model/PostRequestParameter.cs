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
            CdmName = connectorParameter.CdmName;
            Tags = connectorParameter.Tags;
    }

        public object ParamValue { get; set; }
    }
}
