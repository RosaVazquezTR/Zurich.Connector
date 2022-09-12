using Microsoft.AspNetCore.Builder;
using Zurich.Connector.Web.Configuration;

namespace Zurich.Connector.Web.Extensions
{
    public static class MiddlewareExtensions
    {

        /// <summary>
        /// Adds the <see cref="AppTokenTransformationMiddleware"/> to the specified <see cref="IApplicationBuilder"/>, which is necessary for supporting proxy scenarios
        /// </summary>
        /// <param name="builder">The application builder</param>
        /// <returns></returns>
        public static IApplicationBuilder UseClaimsTransformation(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AppTokenTransformationMiddleware>();
        }
    }
}
