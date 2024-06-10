using Asp.Versioning;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Zurich.Common.Exceptions;
using Zurich.Connector.App.Exceptions;
using Zurich.Connector.App.Model;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Web.Models;

namespace Zurich.Connector.Web.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    public class ConnectorsController : ControllerBase
    {
        private readonly IConnectorService _connectorService;
        private readonly IConnectorDataService _connectorDataService;
        private readonly IRegistrationService _registrationService;
        private readonly IMapper _mapper;

        public ConnectorsController(IConnectorService connectorService, IConnectorDataService connectorDataService, IMapper mapper, IRegistrationService registrationService)
        {
            _connectorService = connectorService;
            _connectorDataService = connectorDataService;
            _registrationService = registrationService;
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

                var selectedConnector = _connectorService.GetConnector(id);

                if (selectedConnector.Result.Info.AcceptsSearchWildCard.HasValue)
                {
                    if(!(bool)selectedConnector.Result.Info.AcceptsSearchWildCard && parameters["Query"] == "*")
                    {
                        return new ContentResult
                        {
                            Content = "Connector does not supports the * search wildcard.",
                            ContentType = System.Net.Mime.MediaTypeNames.Application.Json,
                            StatusCode = StatusCodes.Status400BadRequest
                        };
                    }
                }

                if (selectedConnector.Result.Filters != null)
                {
                    foreach (ConnectorsFiltersModel filter in selectedConnector.Result.Filters)
                    {
                        if (filter.IsMultiSelect == "false")
                        {
                            if (parameters.ContainsKey(filter.RequestParameter))
                            {
                                var filterContent = parameters[filter.RequestParameter];
                                if (filterContent.Contains(","))
                                {
                                    return new ContentResult
                                    {
                                        Content = $"{filter.RequestParameter} doesn't support multiple values and received more than one",
                                        ContentType = System.Net.Mime.MediaTypeNames.Application.Json,
                                        StatusCode = StatusCodes.Status400BadRequest
                                    };
                                }
                            }
                        }

                    }
                }

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
                if (e is MaxResultSizeException ||
                    e is RequiredParameterMissingException ||
                    e is InvalidQueryParameterDataType ||
                    e.InnerException is InvalidQueryFormatException)
                {
                    return BadRequest(e.Message);
                }
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
        public async Task<ActionResult<List<ConnectorListViewModel>>> Connectors([FromQuery] ConnectorFilterViewModel filters)
        {
            var filterRequest = _mapper.Map<ConnectorFilterModel>(filters);
            List<ConnectorModel> connections = await _connectorService.GetConnectors(filterRequest);
            List<ConnectorListViewModel> results = _mapper.Map<List<ConnectorListViewModel>>(connections);

            if (results.Count == 0)
            {
                return NoContent();
            }

            var jsonSettings = new JsonSerializerSettings
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter> { new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() } }
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
        /// Sample request:
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

        /// <summary>
        /// Revoke Tenant Application by Connector Id - for all users
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     DELETE /connectors/<ID>/all
        /// </remarks>
        /// <param name="id">
        /// Connector ID
        /// </param>
        /// <returns>
        /// A <see cref="ActionResult"/>ActionResult response</returns>
        /// </returns>
        [HttpDelete("{id}/all")]
        public async Task<ActionResult> RevokeTenantApplication(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }
            if (!string.IsNullOrEmpty(id))
            {
                try
                {
                    var results = await _connectorService.RevokeTenantApplication(id);
                    if (results)
                    {
                        return Ok();
                    }
                }
                catch (Exception)
                {
                    return BadRequest();
                }

            }
            return BadRequest();
        }

        [HttpPost]
        public async Task<ActionResult<DataSourceRegistrationResponseViewModel>> ConnectorRegistration([FromBody] ConnectorRegistrationModel registrationModel)
        {

            if (string.IsNullOrEmpty(registrationModel.ConnectorId))
            {
                return BadRequest("Connector Id must be defined");
            }

            var registrationResult = await _registrationService.RegisterConnector(registrationModel.ConnectorId, registrationModel.Domain);
            if (registrationResult == null)
            {
                return BadRequest();
            }

            var result = _mapper.Map<DataSourceRegistrationResponseViewModel>(registrationResult);

            if (result.Registered || !string.IsNullOrEmpty(result.AuthorizeUrl))
            {
                return Ok(result);
            }
            else
            {
                return BadRequest();
            }
        }

    }
}
