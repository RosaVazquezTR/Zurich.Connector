using System;
using System.Collections.Generic;
using Zurich.Connector.Data.Services;

namespace Zurich.Connector.Data.Factories
{
    public interface IHttpResponseFactory
    {
        IHttpResponseService GetImplementation(string httpResponseType);
    }


    public class HttpResponseFactory : IHttpResponseFactory
    {
        private readonly IServiceProvider serviceProvider;
        public HttpResponseFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IHttpResponseService GetImplementation(string httpResponseType)
        {
            if (httpResponseType.Equals("XML", StringComparison.OrdinalIgnoreCase))
                return (IHttpResponseService)serviceProvider.GetService(typeof(HttpXmlResponseService));
            // JSON
            return (IHttpResponseService)serviceProvider.GetService(typeof(HttpJsonResponseService));
        }
    }
}
