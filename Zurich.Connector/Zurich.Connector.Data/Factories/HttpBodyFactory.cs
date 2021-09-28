using System;
using System.Collections.Generic;
using Zurich.Connector.Data.Services;

namespace Zurich.Connector.Data.Factories
{
    public interface IHttpBodyFactory
    {
        IHttpBodyService GetImplementation(string httpBodyType);
    }


    public class HttpBodyFactory : IHttpBodyFactory
    {
        private readonly IServiceProvider serviceProvider;

        public HttpBodyFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IHttpBodyService GetImplementation(string dataMappingType)
        {
            if (dataMappingType.Equals("POST", StringComparison.OrdinalIgnoreCase))
                return (IHttpBodyService)serviceProvider.GetService(typeof(HttpPostBodyService));
            // GET
            return (IHttpBodyService)serviceProvider.GetService(typeof(HttpGetBodyService));
        }
    }
}
