using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using Zurich.Common.Models;
using Zurich.TenantData;

namespace Zurich.Connector.Web.Controllers
{
    [ApiVersionNeutral]
    [Route("[controller]")]
    [AllowAnonymous]
    [ApiController]
    public class HealthController: ControllerBase
    {
        private readonly TenantContext _context;

        public HealthController(TenantContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult<HealthResponse> Run()
        {
            // Start with everything being unhealthy.
            var statusCode = HttpStatusCode.InternalServerError;
            // TODO: Add cosmos checks in the future (maybe)
            HealthResponse response = new HealthResponse() { Status = HealthResponse.Unhealthy };
            response.IndividualStatus["Database"] = HealthResponse.Unhealthy;

            try
            {
                // Check Database
                CheckDatabase(response);

                // if all values are healthy.
                if (response.IndividualStatus.All(x => x.Value.Equals(HealthResponse.Healthy, StringComparison.OrdinalIgnoreCase)))
                {
                    statusCode = HttpStatusCode.OK;
                    response.Status = HealthResponse.Healthy;
                }
            }
            catch (Exception ex)
            {
                // Try to make a somewhat helpful message if we get an exception
                StackFrame frame;
                // Get stack trace for the exception with source file information
                StackTrace stackTrace = new StackTrace(ex, true);

                var framesWithFile = stackTrace.GetFrames().Where(x => x.GetFileName() != null);
                if (framesWithFile.Any())
                    frame = framesWithFile.First();
                else
                    //first stackFrame
                    frame = stackTrace.GetFrame(0);

                response.Message = $"Message:{ex.Message} File:{frame.GetFileName()} Line:{frame.GetFileLineNumber()} Method:{frame.GetMethod().DeclaringType.Name}";
            }

            return StatusCode((int)statusCode, response);
        }

        private void CheckDatabase(HealthResponse healthResponse)
        {
            if (_context.Database.CanConnect())
            {
                healthResponse.IndividualStatus["Database"] = HealthResponse.Healthy;
            }
        }
    }
}
