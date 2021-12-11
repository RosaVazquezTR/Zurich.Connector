using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Common.Exceptions;
using Zurich.Connector.App.Enum;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Web.Models;

namespace Zurich.Connector.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ConnectorsController : ControllerBase
    {
        private readonly IConnectorService _connectorService;
        private readonly IConnectorDataService _connectorDataService;
        private readonly IRegistrationService _registrationService;
        private readonly IMapper _mapper;
        private readonly ILogger<ConnectorsController> _logger;

        public ConnectorsController(IConnectorService connectorService, IConnectorDataService connectorDataService, ILogger<ConnectorsController> logger, IMapper mapper, IRegistrationService registrationService)
        {
            _connectorService = connectorService;
            _connectorDataService = connectorDataService;
            _registrationService = registrationService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("{id}/data")]
        public async Task<ActionResult<dynamic>> ConnectorData(string id, [FromQuery] string hostName, [FromQuery] string transferToken, bool retrieveFilters)
        {
            dynamic results;
            try
            {
                // TODO: Eventually hostname and transferToken will be removed 
                Dictionary<string, string> parameters = HttpContext?.Request.Query.Keys.Cast<string>()
                    .Where(param => !param.Equals("hostname", StringComparison.InvariantCultureIgnoreCase)
                    && !param.Equals("transferToken", StringComparison.InvariantCultureIgnoreCase)
                    && !param.Equals("retrievefilters", StringComparison.InvariantCultureIgnoreCase))
                    .ToDictionary(k => k, v => HttpContext?.Request.Query[v].ToString(), StringComparer.OrdinalIgnoreCase);

                results = await _connectorDataService.GetConnectorData(id, hostName, transferToken, parameters, retrieveFilters);
                if (results == null)
                {
                    throw new ResourceNotFoundException("Connector or data not found");
                }

                var jsonSettings = new JsonSerializerSettings
                {
                    ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
                };

                // Making dynamic object properties camel cased requires a workaround of converting a JToken to ExpandoObject and then serializing
                // https://briandunnington.github.io/jobject_serialization
                JToken jToken = JToken.FromObject(results);
                dynamic expando = jToken.Type == JTokenType.Array ? jToken.ToObject<List<ExpandoObject>>() : jToken.ToObject<ExpandoObject>();
                var jsonResults = JsonConvert.SerializeObject(expando, jsonSettings);

                return new ContentResult
                {
                    Content = jsonResults,
                    ContentType = System.Net.Mime.MediaTypeNames.Application.Json,
                    StatusCode = StatusCodes.Status200OK
                };

            }
            catch (Exception e)
            {
                return new ContentResult
                {
                    Content = e.Message,
                    ContentType = System.Net.Mime.MediaTypeNames.Application.Json,
                    StatusCode = StatusCodes.Status500InternalServerError
                };
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
        public async Task<ActionResult<List<ConnectorListViewModel>>> Connectors([FromQuery] Common.Models.Connectors.ConnectorFilterModel filters)
        {
            List<ConnectorModel> connections = await _connectorService.GetConnectors(filters);
            List<ConnectorListViewModel> results = _mapper.Map<List<ConnectorListViewModel>>(connections);

            if (results.Count == 0)
            {
                return NoContent();
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
                throw new ResourceNotFoundException($"Connector 'id' not found");
            }
            var responsedata = _mapper.Map<ConnectorViewModel>(results);
            return Ok(responsedata);
        }

        [HttpPost]
        public async Task<ActionResult<ConnectorRegistrationViewModel>> ConnectorRegistration([FromBody] ConnectorRegistrationModel registrationModel)
        {

            if (string.IsNullOrEmpty(registrationModel.ConnectorId))
            {
                return BadRequest("Connector Id must be defined");
            }

            var connector = await _connectorService.GetConnector(registrationModel.ConnectorId);
            ConnectorListViewModel connectorResults = _mapper.Map<ConnectorListViewModel>(connector);
            if (connectorResults == null)
            {
                return BadRequest("Connector Id must point to a valid connector");
            }
            var registered = await _registrationService.RegisterConnector(connectorResults.Id, connectorResults.DataSource.AppCode, connectorResults.DataSource.RegistrationInfo.RegistrationMode);
            if (!registered)
            {
                return BadRequest();
            }

            return Ok(RegistrationStatus.Registered);
        }

        [HttpDelete("{id}/user")]
        public async Task<ActionResult> RemoveConnectorRegistration(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                return BadRequest("Invalid Connector ID");
            }

            var status = await _registrationService.RemoveUserConnector(id);
            if (status == true)
            {
                return NoContent();
            }
            else
            {
                return NotFound($"User registration with connector id {id} was not found");
            }
        }

    }
}
