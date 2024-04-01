using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Cors;
using System.Net;
using Microsoft.AspNetCore.Http;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Factories;
using Zurich.Connector.Data.Model;
using Zurich.Connector.App.Services;
using System.Collections.Generic;

namespace Zurich.Connector.Web.Controllers.V1
{
    /// <summary>
    /// It supports Client Credential flow for the Connectors.
    /// </summary>
    [Route("api/v{version:apiVersion}/download/")]
    [ApiController]
    [ApiVersion("1.0")]
    public class DocumentDownloadController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IDataMapping _dataMapping;
        private readonly IDocumentDownloadService _documentDownloadService;

        public DocumentDownloadController(IHttpClientFactory httpClientFactory, IDataMappingFactory dataMappingFactory, IDocumentDownloadService documentDownloadService)
        {
            _httpClientFactory = httpClientFactory;
            _dataMapping = dataMappingFactory.GetImplementation(AuthType.OAuth2.ToString());
            _documentDownloadService = documentDownloadService;
        }

        /// <summary>
        /// This temporal endpoint acts as a proxy for calling the iManage Document Download Enpoint for the FS UI POC.
        /// </summary>
        /// <param name="connectorId"></param>
        /// <param name="docId"></param>
        /// <returns>Return the document as a FileStreamResult</returns>
        [EnableCors("MainCORS")]
        [HttpGet("{connectorId}/{docId}")]
        public async Task<ActionResult<dynamic>> DocumentDownload(string connectorId, string docId, bool transformToPDF= true)
        {
            string result;
            List<string> supportedConnectors = new List<string> { "44", "14", "80", "47" };
            try
            {
                if(supportedConnectors.Contains(connectorId))
                {
                    result = await _documentDownloadService.GetDocumentContent(connectorId, docId, transformToPDF);

                    return new ContentResult
                    {
                        Content = result,
                        ContentType = Application.Json,
                        StatusCode = StatusCodes.Status200OK
                    };
                }
                else
                {
                    return BadRequest("Unsupported connector");
                }
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError, ex.Message);
            }

        }
    }
}
