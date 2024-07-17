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
using Zurich.Connector.App.Exceptions;

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
        private readonly List<string> SUPPORTED_CONNECTORS = ["44", "14", "80", "47", "12"];


        /// <summary>
        /// This temporal endpoint acts as a proxy for calling the iManage Document Download Enpoint for the FS UI POC.
        /// </summary>
        /// <param name="documentDownloadRequestModel">Document download request model</param>
        /// <param name="transformToPDF">Transform to pdf</param>
        /// <returns>Return the document as a FileStreamResult</returns>
        [EnableCors("MainCORS")]
        [HttpGet("{connectorId}/{docId}")]
        public async Task<ActionResult<dynamic>> DocumentDownload([FromRoute] DocumentDownloadRequestModel documentDownloadRequestModel, [FromQuery] bool transformToPDF = true)
        {
            try
            {
                if (SUPPORTED_CONNECTORS.Contains(documentDownloadRequestModel.ConnectorId))
                {
                    Stream documentStream = await documentDownloadService.GetDocumentContentAsync(documentDownloadRequestModel);

                    string result = await documentDownloadService.GetDocumentContentAsStringAsync(documentStream, transformToPDF);

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