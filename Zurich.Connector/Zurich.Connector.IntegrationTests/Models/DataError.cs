namespace Zurich.Connector.IntegrationTests.Models
{
    public class DataError
    {
        public string TargetSite { get; set; }

        public string StackTrace { get; set; }

        public string Message { get; set; }

        public string InnerException { get; set; }

        public string HelpLink { get; set; }

        public string Source { get; set; }

        public long HResult { get; set; }

    }
}
