using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Zurich.Connector.App.Model;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Web.Models;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data.Repositories.CosmosDocuments;
using Zurich.Connector.App.Enum;
using Microsoft.OpenApi.Extensions;
using Zurich.Common.Exceptions;
using System.Net;

namespace Zurich.Connector.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConnectorsController : ControllerBase
    {
        private readonly IConnectorService _connectorService;
        private readonly IRegistrationService _registrationService;
        private readonly IMapper _mapper;
        private readonly ILogger<ConnectorsController> _logger;

        public ConnectorsController(IConnectorService connectorService, ILogger<ConnectorsController> logger, IMapper mapper,  IRegistrationService registrationService)
        {
            _connectorService = connectorService;
            _registrationService = registrationService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("{id}/data")]
        public async Task<ActionResult<dynamic>> ConnectorData(string id, [FromQuery] string hostName, [FromQuery] string transferToken, bool retriveFilter)
        {
            dynamic results;
            try { 
                // TODO: Eventually hostname and transferToken will be removed 
                Dictionary<string, string> parameters = HttpContext?.Request.Query.Keys.Cast<string>()
                    .Where(param => !param.Equals("hostname", StringComparison.InvariantCultureIgnoreCase) 
                    && !param.Equals("transferToken", StringComparison.InvariantCultureIgnoreCase) 
                    && !param.Equals("retrievefilters", StringComparison.InvariantCultureIgnoreCase))
                    .ToDictionary(k => k, v => HttpContext?.Request.Query[v].ToString());

                results = await _connectorService.GetConnectorData(id, hostName, transferToken, parameters, retriveFilter);
                if (results == null)
                {
                    throw  new ResourceNotFoundException("Connector or data not found");
                }
                var jsonResults = JsonConvert.SerializeObject(results);
                return new ContentResult
                {
                    Content = jsonResults,
                    ContentType = System.Net.Mime.MediaTypeNames.Application.Json,
                    StatusCode = StatusCodes.Status200OK
                };
            }
            catch (Exception e)
            {
                return new Exception(e.Message);
            }

        }

        /// <summary>
        /// API to get a list of connectors
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///		GET /connectors?<params>
        ///		
        /// </remarks>
        /// <param name="filters">
        /// 1. entityType is optional parameter to filter to specific data types of connectors e.g. Document, Search, etc.
        /// 2. registrationMode is optional parameter to filter by specific registration mode for the connectors, e.g.registered would return all connectors that are registered for the user.
        /// 3. dataSource is optional parameter to filter by specific data source.
        /// </param>
        /// <returns>
        /// A <see cref="ConnectorListViewModel"/> with the connectors</returns>
        /// id (this is a generic id for the specific connector used by all users), entityType, dataSource, registrationMode(registered/autoReg/manualReg), registeredOn (optional date time), domain (optional)
        /// </returns>
        /// <response code="200">A <see cref="ConnectorListViewModel"/> representing the connectors</response>

        [HttpGet()]
        public async Task<ActionResult<List<ConnectorListViewModel>>> Connectors([FromQuery] ConnectorFilterModel filters)
        {
            List<ConnectorModel> connections = await _connectorService.GetConnectors(filters);
            List<ConnectorListViewModel> results = _mapper.Map<List<ConnectorListViewModel>>(connections);

            if(results.Count == 0)
            {
                throw new ResourceNotFoundException("Connector or data not found");
            }

            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            var jsonResults = JsonConvert.SerializeObject(results, jsonSettings);
            return new ContentResult
            {
                Content = jsonResults,
                ContentType = System.Net.Mime.MediaTypeNames.Application.Json,
                StatusCode = StatusCodes.Status200OK
            };
        }

        /// <summary>
        /// Get connector definition - Returns the configuration of the connector, configuration is generic for all users
        /// </summary>
        /// <remarks>
        /// Sample reque3st:
        ///     GET /connectors/<ID>
        /// </remarks>
        /// <param name="id">
        /// Connector ID
        /// </param>
        /// <returns>
        /// A <see cref="ConnectorViewModel"/> with the connectors</returns>
        /// (includes connector authorization and base configuration, CDM returned, data of request and response for external API, 
        /// filters, extra set up – specific to search sorting and query parameters)
        /// </returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ConnectorViewModel>> Connectors(string id)
        {
            var results = await _connectorService.GetConnector(id);
            if (results == null)
            {
                throw new  ResourceNotFoundException($"Connector 'id' not found");
            }
            var responsedata = _mapper.Map<ConnectorViewModel>(results);
            return Ok(responsedata);
        }

        [HttpPost]
        public async Task<ActionResult<ConnectorRegistrationViewModel>>ConnectorRegistration([FromBody] ConnectorRegistrationModel registrationModel)
        {
            try
            {
                await _registrationService.RegisterDataSource(registrationModel.DataSourceId, registrationModel.ConnectorId);
                return Ok(RegistrationStatus.register);
            }
            catch(Exception ex)
            {
                return BadRequest(RegistrationStatus.notRegister);
                
            }
        }

        [HttpDelete("{dataSourceId}")]
        public async Task<ActionResult> DeleteConnectorAsync(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return await Task.FromResult(StatusCode((int)HttpStatusCode.BadRequest));
            }
            await _registrationService.RemoveUserConnector(id);
            return Ok(HttpStatusCode.OK);
        }

    }
}
