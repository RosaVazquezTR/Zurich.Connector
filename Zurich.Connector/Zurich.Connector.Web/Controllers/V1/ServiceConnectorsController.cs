using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using Zurich.Connector.Web.Configuration;

namespace Zurich.Connector.Web.Controllers.V1
{
    /// <summary>
    /// It supports Client Credential flow for the Connectors.
    /// </summary>
    [Route("api/v{version:apiVersion}/service/connectors")]
    [ApiController]
    [ApiVersion("1.0")]
    public class ServiceConnectorsController : ControllerBase
    {
        private readonly IConnectorService _connectorService;
        private readonly IConnectorDataService _connectorDataService;
        private readonly IRegistrationService _registrationService;
        private readonly IMapper _mapper;

        public ServiceConnectorsController(IConnectorService connectorService, IConnectorDataService connectorDataService, IMapper mapper, IRegistrationService registrationService)
        {
            _connectorService = connectorService;
            _connectorDataService = connectorDataService;
            _registrationService = registrationService;
            _mapper = mapper;
        }

        /// <summary>
        /// This makes call to the ConnectorData and returns the dynamic results based on the connector configuration.
        /// The parameter {tenantId} is requried so that <see cref="Zurich.Connector.Web.Configuration.AppTokenTransformationMiddleware"/> 
        /// will add the TenantId in the HttpContext and can be retrieved across the solutions using <see cref="Zurich.TenantData.ISessionAccessor"/> 
        /// Also the token should have connectors.appfull scope to invoke this endpoint.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="hostName"></param>
        /// <param name="transferToken"></param>
        /// <param name="retrieveFilters"></param>
        /// <param name="tenantId"></param>
        /// <returns></returns>
        [Authorize(Policy = Policies.ServiceConnectorPolicy)]
        [HttpGet("{id}/data/{tenantId}")]
        public async Task<ActionResult<dynamic>> ConnectorData(string id, [FromQuery] string hostName, [FromQuery] string transferToken, bool retrieveFilters, string tenantId)
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
                if (e is MaxResultSizeException)
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
    }
}
