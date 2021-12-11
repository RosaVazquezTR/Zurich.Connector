using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Zurich.Common.Models;
using Zurich.Connector.App.Services;

namespace Zurich.Connector.Durable
{
    public class HealthFunction
	{
        private readonly ICosmosService _cosmosService;

        public HealthFunction(ICosmosService cosmosService)
        {
            _cosmosService = cosmosService;
        }

        [Function("Health")]
		public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = null)] HttpRequestData req,
            FunctionContext executionContext)
        {
			// Start with everything being unhealthy.
			var statusCode = HttpStatusCode.InternalServerError;
			HealthResponse response = new HealthResponse() { Status = HealthResponse.Unhealthy };
			response.IndividualStatus["Cosmos"] = HealthResponse.Unhealthy;

            try
            {
                // Check Cosmos
                await this.CheckCosmos(response);
                // if all values are healthy.
                if (response.IndividualStatus.All(x => x.Value.Equals(HealthResponse.Healthy, System.StringComparison.OrdinalIgnoreCase)))
                {
                    statusCode = HttpStatusCode.OK;
                    response.Status = HealthResponse.Healthy;
                }
            }
            catch (Exception ex)
            {
                response.Message = ex.Message;
                // Try to make a somewhat helpful message if we get an exception
                StackFrame frame;
                // Get stack trace for the exception with source file information
                StackTrace stackTrace = new StackTrace(ex, true);
                // Get the top stack frame
                IEnumerable<StackFrame> framesWithFile = stackTrace.GetFrames().Where(x => x.GetFileName() != null);
                if (framesWithFile.Any())
                {
                    frame = framesWithFile.First();
                }
                else
                {
                    //first stackFrame
                    frame = stackTrace.GetFrame(0);
                }

                response.Message = $"Message:{ex.Message} File:{frame.GetFileName()} Line:{frame.GetFileLineNumber()} Method:{frame.GetMethod().DeclaringType.Name}";
            }

            var httpResponse = req.CreateResponse(statusCode);
            await httpResponse.WriteAsJsonAsync<HealthResponse>(response);

            return httpResponse;
        }

        private async Task CheckCosmos(HealthResponse healthResponse)
        {
            // probably not the best way but check if we can get a datasource.
            var value = await _cosmosService.GetDataSource("10");
            if (value != null)
            {
                healthResponse.IndividualStatus["Cosmos"] = HealthResponse.Healthy;
            }
        }

    }
}
