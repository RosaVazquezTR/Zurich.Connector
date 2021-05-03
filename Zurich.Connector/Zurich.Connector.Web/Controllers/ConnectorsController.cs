using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Threading.Tasks;

using Zurich.Connector.Data.Services;

namespace Zurich.Connector.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConnectorsController : ControllerBase
    {
        private readonly IConnectorService _connectorService;
        private readonly ILogger<ConnectorsController> _logger;

        public ConnectorsController(IConnectorService connectorService, ILogger<ConnectorsController> logger)
        {
            _connectorService = connectorService;
            _logger = logger;
        }

        [HttpGet("{id}/data")]
        public async Task<ActionResult<dynamic>> ConnectorData(string id, [FromQuery] string hostname, [FromQuery] string transferToken)
        {
            var results = await _connectorService.GetConnectorData(id, hostname, transferToken);
            if (results == null)
            {
                return NotFound("Connector or data not found");
            }

            var jsonResults = JsonConvert.SerializeObject(results);
            return new ContentResult
            {
                Content = jsonResults,
                ContentType = System.Net.Mime.MediaTypeNames.Application.Json,
                StatusCode = StatusCodes.Status200OK
            };
        }

    }
}
