using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Zurich.Connector.App.Enum;
using Zurich.Connector.App.Services;
using Zurich.Connector.Data.Services;
using Zurich.Connector.Web.Models;

namespace Zurich.Connector.Web.Controllers
{
    [ApiController]
    [Route("api/v{version:apiVersion}/")]
    [ApiVersion("1.0")]
    public class RegistrationController : ControllerBase
    {
        private readonly IConnectorService _connectorService;
        private readonly IRegistrationService _registrationService;
        private readonly IMapper _mapper;
        private readonly ILogger<RegistrationController> _logger;

        public RegistrationController(IConnectorService connectorService, ILogger<RegistrationController> logger, IMapper mapper, IRegistrationService registrationService)
        {
            _connectorService = connectorService;
            _registrationService = registrationService;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Registers a datasource for connectors.  The datasource will be used to find the registered connectors
        /// </summary>
        /// <remarks>
        /// Sample request:
        ///     Post /api/v1/datasource/iManage<ID>
        /// </remarks>
        /// <param name="applicationCode">
        /// The application to register
        /// </param>
        /// <param name="registrationModel">
        /// Additional information used to register a given domain
        /// </param>
        /// <returns>
        /// A <see cref="DataSourceRegistrationResponseViewModel"/> with authorizeUrl if required
        /// </returns>
        [HttpPost("datasource/{applicationCode}")]
        public async Task<ActionResult<DataSourceRegistrationResponseViewModel>> DataSourceRegistration(string applicationCode, [FromBody] DataSourceRegistrationRequestViewModel registrationModel)
        {

            if (string.IsNullOrEmpty(applicationCode))
            {
                return BadRequest("applicationCode must be included");
            }

            var registrationResult = await _registrationService.RegisterDataSource(applicationCode, registrationModel?.Domain, null);
            var result = _mapper.Map<DataSourceRegistrationResponseViewModel>(registrationResult);

            if (result.Registered || !string.IsNullOrEmpty(result.AuthorizeUrl))
            {
                return Ok(result);
            }
            else
            {
                return BadRequest($"Cannot register {applicationCode}");
            }
        }
    }
}
