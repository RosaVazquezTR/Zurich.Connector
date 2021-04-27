using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.DataMap;
using Zurich.Connector.Data.Model;

namespace Zurich.Connector.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConnectorController : ControllerBase
    {
        private IDataMapping _dataMapping;
        public ConnectorController(IDataMapping dataMapping)
        {
            _dataMapping = dataMapping;
        }

        [HttpGet]
        public async Task<ActionResult<dynamic>> ConnectorData([FromQuery] int connectorId, [FromQuery] string transferToken, [FromQuery] string hostname)
        {
            var query = Request.Query;
            //var connectorId = query["id"];
            //var transferToken = query["transferToken"];
            //var hostname = query["hostname"];

            var response = await _dataMapping.Get<dynamic>("PLCUS", DataType.History, transferToken);
            //foreach (var matter in response)
            //{
            //    // Hardcoding in domain for now. Will have to handle dynamic domain soon.
            //    matter.WebLink = $"https://cloudimanage.com/work/link/w/{matter.Id}";
            //}
            var jsonResults = JsonConvert.SerializeObject(response);
            //return Ok(jsonResults);
            return new ContentResult
            {
                Content = jsonResults,
                ContentType = System.Net.Mime.MediaTypeNames.Application.Json,
                StatusCode = StatusCodes.Status200OK
            };
        }

        //[HttpGet]
        //public async Task<ActionResult<dynamic>> ConnectorData([FromQuery] int connectorId, [FromQuery] string transferToken, [FromQuery] string hostname)
        //{
        //    var query = Request.Query;
        //    //var connectorId = query["id"];
        //    //var transferToken = query["transferToken"];
        //    //var hostname = query["hostname"];

        //    var response = await _dataMapping.Get<dynamic>("iManage", DataType.Matters, "dsdfsd");
        //    //foreach (var matter in response)
        //    //{
        //    //    // Hardcoding in domain for now. Will have to handle dynamic domain soon.
        //    //    matter.WebLink = $"https://cloudimanage.com/work/link/w/{matter.Id}";
        //    //}
        //    var jsonResults = JsonConvert.SerializeObject(response);
        //    //return Ok(jsonResults);
        //    return new ContentResult
        //    {
        //        Content = jsonResults,
        //        ContentType = System.Net.Mime.MediaTypeNames.Application.Json,
        //        StatusCode = StatusCodes.Status200OK
        //    };
        //}

        //[HttpGet]
        //public async Task<HttpResponseMessage> ConnectorData([FromQuery] int connectorId, [FromQuery] string transferToken, [FromQuery] string hostname)
        //{
        //    var query = Request.Query;
        //    //var connectorId = query["id"];
        //    //var transferToken = query["transferToken"];
        //    //var hostname = query["hostname"];

        //    var response = await _dataMapping.Get<dynamic>("iManage", DataType.Matters);
        //    //foreach (var matter in response)
        //    //{
        //    //    // Hardcoding in domain for now. Will have to handle dynamic domain soon.
        //    //    matter.WebLink = $"https://cloudimanage.com/work/link/w/{matter.Id}";
        //    //}

        //    var jsonResults = JsonConvert.SerializeObject(response);
        //    return new HttpResponseMessage(HttpStatusCode.OK)
        //    {
        //        Content = new StringContent(jsonResults, Encoding.UTF8, MediaTypeNames.Application.Json)
        //    };
        //}
    }
}
