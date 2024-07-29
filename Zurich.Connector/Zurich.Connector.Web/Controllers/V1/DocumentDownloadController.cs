using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Microsoft.AspNetCore.Cors;
using System.Net;
using Microsoft.AspNetCore.Http;
using Zurich.Connector.App.Services;
using System.Collections.Generic;
using Asp.Versioning;
using Zurich.Connector.App.Model;
using System.IO;

namespace Zurich.Connector.Web.Controllers.V1
{
    /// <summary>
    /// It supports Client Credential flow for the Connectors.
    /// </summary>
    [Route("api/v{version:apiVersion}/download/")]
    [ApiController]
    [ApiVersion("1.0")]
    public class DocumentDownloadController(IDocumentDownloadService documentDownloadService) : ControllerBase
    {
        private readonly List<string> SUPPORTED_CONNECTORS = ["44", "89", "14", "80", "47", "12"];

        /// <summary>
        /// Downloads a document for a specific connector.
        /// </summary>
        /// <param name="connectorId">The ID of the connector.</param>
        /// <param name="docId">The ID of the document.</param>
        /// <param name="transformToPdf">Flag indicating whether to transform the document to PDF. Default is true.</param>
        /// <returns>The document content as json.</returns>
        [HttpGet("{connectorId}/{docId}")]
        public async Task<ActionResult<dynamic>> DocumentDownload(string connectorId, string docId, [FromQuery] bool transformToPdf = true)
        {
            try
            {
                DocumentDownloadRequestModel documentDownloadRequestModel = new()
                {
                    ConnectorId = connectorId,
                    DocId = docId
                };

                if (SUPPORTED_CONNECTORS.Contains(documentDownloadRequestModel.ConnectorId))
                {
                    string result = await documentDownloadService.GetDocumentContentAsStringAsync(documentDownloadRequestModel, transformToPdf);

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