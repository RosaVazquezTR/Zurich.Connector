using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Zurich.Connector.Data.DataMap;

namespace Zurich.Connector.Web.Controllers
{
    [ApiController]
    [Route("[ConnectorData]")]
    public class ConnectorController : Controller
    {
        internal IDataMapping _dataMapping;
        public ConnectorController(IDataMapping dataMapping)
        {
            _dataMapping = dataMapping;
        }

        [HttpGet]
        public async Task<HttpResponseMessage> ConnectorData()
        {
            var query = Request.Query;
            var connectorId = query["id"];
            var transferToken = query["transferToken"];
            var hostname = query["hostname"];

            var response = await _dataMapping.Get(appCode, DataType.Matters);
            foreach (var matter in response)
            {
                // Hardcoding in domain for now. Will have to handle dynamic domain soon.
                matter.WebLink = $"https://cloudimanage.com/work/link/w/{matter.Id}";
            }
            return response;
        }
    }
}
