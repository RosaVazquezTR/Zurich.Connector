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
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Web.Models;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Configuration;

namespace Zurich.Connector.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConnectorsController : ControllerBase
    {
        private readonly IConnectorService _connectorService;
        private readonly IMapper _mapper;
        private readonly ILogger<ConnectorsController> _logger;

        public ConnectorsController(IConnectorService connectorService, ILogger<ConnectorsController> logger, IMapper mapper)
        {
            _connectorService = connectorService;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("{id}/data")]
        public async Task<ActionResult<dynamic>> ConnectorData(string id, [FromQuery] string hostName, [FromQuery] string transferToken)
        {
            dynamic results;
            try
            {
                // TODO: Eventually hostname and transferToken will be removed 
                Dictionary<string, string> parameters = HttpContext?.Request.Query.Keys.Cast<string>().Where(param => !param.Equals("hostname", StringComparison.InvariantCultureIgnoreCase) && !param.Equals("transferToken", StringComparison.InvariantCultureIgnoreCase)).ToDictionary(k => k, v => HttpContext?.Request.Query[v].ToString());
                results = await _connectorService.GetConnectorData(id, hostName, transferToken, parameters);
                if (results == null)
                {
                    return NotFound("Connector or data not found");
                }
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }

            var jsonResults = JsonConvert.SerializeObject(results);
            return new ContentResult
            {
                Content = jsonResults,
                ContentType = System.Net.Mime.MediaTypeNames.Application.Json,
                StatusCode = StatusCodes.Status200OK
            };
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
        /// A <see cref="ConnectorViewModel"/> with the connectors</returns>
        /// id (this is a generic id for the specific connector used by all users), entityType, dataSource, registrationMode(registered/autoReg/manualReg), registeredOn (optional date time), domain (optional)
        /// </returns>
        /// <response code="200">A <see cref="ConnectorViewModel"/> representing the connectors</response>

        [HttpGet()]
        public async Task<ActionResult<List<ConnectorViewModel>>> Connectors([FromQuery] ConnectorFilterModel filters)
        {
            List<ConnectorModel> connections = await _connectorService.GetConnectors(filters);
            List<ConnectorViewModel> results = _mapper.Map<List<ConnectorViewModel>>(connections);

            if(results.Count == 0)
            {
                return NotFound();
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
        [HttpGet("{id}")]
        public async Task<ActionResult<ConnectorConfigViewModel>> ConnectorconfigData([FromQuery] string Connectorid)
        {
            var results = await _connectorService.GetConnectorConfiguration(Connectorid);
            if (results == null)
            {
                return NotFound("Connector not found");
            }
            var responsedata = _mapper.Map<ConnectorConfigViewModel>(results);
            return Ok(responsedata);
        }

    }
}
