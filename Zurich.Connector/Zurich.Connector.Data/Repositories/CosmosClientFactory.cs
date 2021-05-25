using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using System;
using Zurich.Common.Models.Cosmos;

namespace Zurich.Connector.Data.Repositories
{
    public interface ICosmosClientFactory
	{
		/// <summary>
		/// Get cosmos client instnace
		/// </summary>
		CosmosClient CosmosClient { get; }
		/// <summary>
		/// Get a document client instance.
		/// </summary>
		DocumentClient DocumentClient { get; }
	}

	/// <summary>
	/// Creates and disposes cosmos clients and documents.
	/// In the future, we may want to expand this to use a dictionary, so it can maintain 
	/// clients for different cosmos databases and consumers could retrieve them by name.
	/// </summary>
	public class CosmosClientFactory : ICosmosClientFactory, IDisposable
	{
		private bool _disposedValue;

		public CosmosClient CosmosClient { get; private set; }
		public DocumentClient DocumentClient { get; private set; }

		public CosmosClientFactory(CosmosDbOptions dbOptions, CosmosClientOptions clientOptions)
		{
			DocumentClient = new DocumentClient(new Uri(dbOptions.Endpoint), dbOptions.PrimaryKey);
			CosmosClient = new CosmosClient(dbOptions.Endpoint, dbOptions.PrimaryKey, clientOptions);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposedValue)
			{
				if (disposing)
				{
					DocumentClient.Dispose();
					CosmosClient.Dispose();
				}
				_disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
