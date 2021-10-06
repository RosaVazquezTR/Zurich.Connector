using AutoMapper;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Zurich.Common.Models.Cosmos;
using Zurich.Common.Repositories.Cosmos;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.Durable.Constants;
using Zurich.Connector.Durable.Model;

namespace Zurich.Connector.Durable.Service
{
	/// <summary>
	/// Service to get and update dynamic filter properties
	/// </summary>
	public interface IFilterService
    {
		/// <summary>
		/// Get a list of connectors which is having dynamic filters.
		/// </summary>
		/// <returns></returns>
		public Task<List<ConnectorModel>> GetDynamicFilterConnectors();
		/// <summary>
		/// Update the dynamic filter with new data
		/// </summary>
		/// <param name="connector"></param>
		/// <param name="dynamicFilter"></param>
		/// <returns></returns>
		public Task UpdateDynamicFilter(ConnectorModel connector, IEnumerable<TaxonomyOptions> dynamicFilter);

	}
    public class FilterService: IFilterService
    {
		private readonly ICosmosService _cosmosService;
		private readonly ILogger _logger;
		private readonly IMapper _mapper;

		public FilterService(ICosmosService cosmosService,
							 IMapper mapper,
							 ILogger<FilterService> logger)
		{
			_cosmosService = cosmosService;
			_logger = logger;
			_mapper = mapper;
		}

		/// <summary>
		/// Fetch all connectors from Cosmos which is having dynamic filters.
		/// </summary>
		/// <returns>Connector document list.</returns> 
		public async Task<List<ConnectorModel>> GetDynamicFilterConnectors()
		{
			List<ConnectorModel> dynamicFilterConnectos = null;
			try
			{
				var connectors = await _cosmosService.GetConnectors(true);
				dynamicFilterConnectos = connectors.Where(t => t.Info.IsDynamicFilter.HasValue && t.Info.IsDynamicFilter.Value == true).ToList();
			}
			catch(Exception ex)
            {
				_logger.Log(LogLevel.Error, "An error occurred while retrieving connectors", ex);
				throw;
            }
			return dynamicFilterConnectos;
		}

		/// <summary>
		/// Update connector with dynamic filters.
		/// </summary>
		/// <param name="connector"></param>
		/// <param name="dynamicFilter"></param>
		/// <returns></returns>
		public async Task UpdateDynamicFilter(ConnectorModel connector, IEnumerable<TaxonomyOptions> dynamicFilter)
        {
			try
			{
				var connectorDocument = _mapper.Map<ConnectorDocument>(connector);
				var filters = connectorDocument.Filters.FirstOrDefault();
				if (filters != null)
				{
					filters.FilterList = _mapper.Map<List<FilterList>>(dynamicFilter);
					await _cosmosService.StoreConnector(connectorDocument);
				}
			}
			catch(Exception ex)
            {
				_logger.Log(LogLevel.Error, "An error occured while updating connector");
				throw;
            }
        }
	}
}
