using Asp.Versioning;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System;
using Zurich.Connector.App.Services;
using static System.Net.Mime.MediaTypeNames;
using Zurich.Connector.App.Model;
using System.Linq;
using System.IO;
using Zurich.Connector.App.Exceptions;

namespace Zurich.Connector.Web.Controllers.V2
{
    [Route("api/v{version:apiVersion}/download/")]
    [ApiController]
    [ApiVersion("2.0")]
    public class DocumentDownloadController(IDocumentDownloadService documentDownloadService) : ControllerBase
    {
        private readonly List<string> SUPPORTED_CONNECTORS = ["44", "14", "80", "47", "12"];

        /// <summary>
        /// This temporal endpoint acts as a proxy for calling the iManage Document Download Enpoint for the FS UI POC.
        /// </summary>
        /// <param name="connectorId">Connector id</param>
        /// <param name="docId">Document id</param>
        /// <returns>Return the document as a FileStreamResult</returns>
        [EnableCors("MainCORS")]
        [HttpGet("{connectorId}/{docId}")]
        public async Task<ActionResult> DocumentDownload([FromRoute] DocumentDownloadRequestModel documentDownloadRequestModel)
        {
            try
            {
                if (SUPPORTED_CONNECTORS.Contains(documentDownloadRequestModel.ConnectorId))
                {
                    Stream file = await documentDownloadService.GetDocumentContentAsync(documentDownloadRequestModel);

                    return new FileStreamResult(file, "application/octet-stream");
                }
                else
                {
                    return BadRequest("Unsupported connector");
                }
            }
            catch (Exception ex)
            {
                int statusCode = ex switch
                {
                    KeyNotFoundException => (int)HttpStatusCode.NotFound,
                    _ => (int)HttpStatusCode.InternalServerError
                };

                return StatusCode(statusCode, ex.Message);
            }
        }
    }
}
