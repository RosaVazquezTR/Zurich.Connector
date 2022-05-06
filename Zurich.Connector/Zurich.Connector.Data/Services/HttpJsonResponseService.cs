namespace Zurich.Connector.Data.Services
{
    public class HttpJsonResponseService : AbstractHttpResponseService, IHttpResponseService
    {
        public HttpJsonResponseService()
        {
            MapResponse = true;
        }
    }
}
