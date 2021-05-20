using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zurich.Connector.Data.Model;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Web.Models;

namespace Zurich.Connector.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConnectorsController : ControllerBase
    {
        private readonly IConnectorService _connectorService;
        private readonly IMapper _mapper;
        private readonly ILogger<ConnectorsController> _logger;

        public ConnectorsController(IConnectorService connectorService, ILogger<ConnectorsController> logger , IMapper mapper)
        {
            _connectorService = connectorService;
            _mapper = mapper;
            _logger = logger;
            _mapper = mapper;
        }

        [HttpGet("{id}/data")]
        public async Task<ActionResult<dynamic>> ConnectorData(string id, [FromQuery] string hostname, [FromQuery] string transferToken)
        {
            dynamic results;
            try
            {
                results = await _connectorService.GetConnectorData(id, hostname, transferToken);
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

        [HttpGet()]
        public async Task<ActionResult<List<ConnectorViewModel>>> Connectors([FromQuery] ConnectorFilterModel filters)
        {
            List<DataMappingConnection> connections = await _connectorService.GetConnectors(filters);
            List<ConnectorViewModel> results = _mapper.Map<List<ConnectorViewModel>>(connections);

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
                return NotFound("Connector or data not found");
            }
            var responsedata = _mapper.Map<ConnectorConfigViewModel>(results);
            return Ok(responsedata);
        }

    }
}
