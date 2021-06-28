namespace Zurich.Connector.Data.Repositories.CosmosDocuments
{
	/// <summary>
	/// Cosmos constants definition
	/// </summary>
    public class CosmosConstants
    {
		// Containers
		public const string ConnectorContainerId = "connector";
		public const string DataSourceContainerId = "datasource";

		// Partitions
		public const string ConnectorPartitionKey = "ConnectorList";
		public const string DataSourcePartitionKey = "DataSourceList";
	}
}
