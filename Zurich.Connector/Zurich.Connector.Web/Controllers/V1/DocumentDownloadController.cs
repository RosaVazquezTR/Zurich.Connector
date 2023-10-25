using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Zurich.Connector.Web.Configuration;
using System.IO;
using PdfiumViewer;
using static System.Net.Mime.MediaTypeNames;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Cors;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;

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

        public DocumentDownloadController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// This temporal endpoint acts as a proxy for calling the iManage Document Download Enpoint for the FS UI POC.
        /// </summary>
        /// <param name="applicationCode"></param>
        /// <param name="docId"></param>
        /// <param name="accessToken"></param>
        /// <returns>Return the document as a FileStreamResult</returns>
        [Authorize(Policy = Policies.ServiceConnectorPolicy)]
        [EnableCors("MainCORS")]
        [HttpGet("{applicationCode}/{docId}/{accessToken}")]
        public async Task<ActionResult<dynamic>> DocumentDownload(string applicationCode, string docId, string accessToken)
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient();

                var apiEndpoint = "https://cloudimanage.com/work/api/v2/customers/241/libraries/USERDB/documents/DOCID/download";
                string database = docId.Split("!")[0];
                apiEndpoint = apiEndpoint.Replace("DOCID", docId).Replace("USERDB", database);
                httpClient.DefaultRequestHeaders.Add("X-Auth-Token", accessToken);

                var response = await httpClient.GetAsync(apiEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    var documentStream = await response.Content.ReadAsStreamAsync();

                    string text = "";
                    string base64String = "";
                    JObject document = new JObject();
                    JObject pageText = new JObject();

                    using (var memoryStream = new MemoryStream())
                    {
                        await documentStream.CopyToAsync(memoryStream);
                        byte[] pdfBytes = memoryStream.ToArray();

                        // Convert the byte array to a Base64 string
                        base64String = Convert.ToBase64String(pdfBytes);
                    }

                    using (PdfDocument pdfDocument = PdfDocument.Load(documentStream))
                    {
                        if (pdfDocument.PageCount > 0)
                        {
                            for (int i = 0; i < pdfDocument.PageCount; ++i)
                            {
                                text = pdfDocument.GetPdfText(i);
                                pageText.Add((i + 1).ToString(), text);
                                
                            }
                        }
                    }
                    
                    document.Add("documentContent", pageText);
                    document.Add("documentBase64", base64String);
                    return new ContentResult
                    {
                        Content = document.ToString(),
                        ContentType = System.Net.Mime.MediaTypeNames.Application.Json,
                        StatusCode = StatusCodes.Status200OK
                    };
                }
                else
                {
                    return StatusCode((int)response.StatusCode, "Failed to download document.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }

        }
    }
}
