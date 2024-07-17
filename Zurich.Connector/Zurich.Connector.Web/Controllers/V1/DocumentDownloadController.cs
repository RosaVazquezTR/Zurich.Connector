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
    public class DocumentDownloadController : ControllerBase
    {
        private readonly IDocumentDownloadService _documentDownloadService;
        private readonly List<string> SUPPORTED_CONNECTORS = ["44", "14", "80", "47", "12"];

        public DocumentDownloadController(IDocumentDownloadService documentDownloadService)
        {
            _documentDownloadService = documentDownloadService;
        }

        /// <summary>
        /// This temporal endpoint acts as a proxy for calling the iManage Document Download Enpoint for the FS UI POC.
        /// </summary>
        /// <param name="connectorId">Connector id</param>
        /// <param name="docId">Document id</param>
        /// <param name="transformToPDF">Transform to pdf</param>
        /// <returns>Return the document as a FileStreamResult</returns>
        [EnableCors("MainCORS")]
        [HttpGet("{connectorId}/{docId}")]
        public async Task<ActionResult<dynamic>> DocumentDownload(string connectorId, string docId, [FromQuery] bool transformToPDF = true)
        {
            try
            {
                var documentDownloadRequestModel = new DocumentDownloadRequestModel {
                    ConnectorId = connectorId,
                    DocId = docId
                };

                if (SUPPORTED_CONNECTORS.Contains(documentDownloadRequestModel.ConnectorId))
                {
                    Stream documentStream = await _documentDownloadService.GetDocumentContentAsync(documentDownloadRequestModel);

                    string result = await _documentDownloadService.GetDocumentContentAsStringAsync(documentStream, transformToPDF);

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