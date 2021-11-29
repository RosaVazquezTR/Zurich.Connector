using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using System.Threading.Tasks;
using Zurich.Connector.Durable.Service;

namespace Zurich.Connector.Durable
{
    public class ConnectorTimerTrigger
    {
        private IPLService _plService;
        private IFilterService _cosmosService;
        public ConnectorTimerTrigger(IPLService plService, IFilterService cosmosService)
        {
            _plService = plService;
            _cosmosService = cosmosService;
        }


        [Function("UpdateDynamicFilter")]
        public async void UpdateDynamicFilter([TimerTrigger("%schedule%")] DurableInfo durableTimer, FunctionContext context)
        {
            var logger = context.GetLogger("Function1");

            if (durableTimer.IsPastDue)
            {
                logger.LogInformation("Timer is running late!");
            }

            await UpdateDynamicFilter();

            logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            logger.LogInformation($"Next timer schedule at: {durableTimer.ScheduleStatus.Next}");
        }

        /// <summary>
        /// Facing some issues when debugging timertrigger functions. Keep the below method till the issue is fixed in visual studio
        /// https://github.com/Azure/azure-functions-dotnet-worker/issues/434
        /// </summary>
        /// <param name="req"></param>
        /// <param name="executionContext"></param>
        /// <returns></returns>
        [Function("TestUpdateDynamicFilter")]
        public async Task<HttpResponseData> TestUpdateDynamicFilter([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            await UpdateDynamicFilter();
            return req.CreateResponse(HttpStatusCode.OK); ;
        }

        private async Task UpdateDynamicFilter()
        {
            var dynamicFilterConnectors = await _cosmosService.GetDynamicFilterConnectors();
            foreach (var connector in dynamicFilterConnectors)
            {
                var dynamicFilter = await _plService.GetPLDynamicFilterList(connector.DataSource.Locale);
                await _cosmosService.UpdateDynamicFilter(connector, dynamicFilter);
            }
        }
    }

    /// <summary>
    /// Durable timer information.
    /// </summary>
    public class DurableInfo
    {
        public DurableScheduleStatus ScheduleStatus { get; set; }

        public bool IsPastDue { get; set; }
    }

    /// <summary>
    /// Durable schedule status.
    /// </summary>
    public class DurableScheduleStatus
    {
        public DateTime Last { get; set; }

        public DateTime Next { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
