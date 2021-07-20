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
		public const string ConnectorRegistrationContainerId = "connectorRegistration";
		// Partitions
		public const string ConnectorPartitionKey = "ConnectorList";
		public const string DataSourcePartitionKey = "DataSourceList";
		public const string ConnectorRegistrationPartitionKey = "ConnectorRegistrationList";
	}
}
