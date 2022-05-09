using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.App
{
    public static class Constants
    {
        public const string filters = "filters";
    }

    public static class AppSettings
    {
        public const string OAuthUrl = "OAuthBaseUrl";
        public const string ShowPreReleaseConnectors = "ShowPreReleaseConnectors";
        public const string InstanceLimit = "InstanceLimit";
        public const string MaxRecordSizePerInstance = "MaxRecordSizePerInstance";
    }

    public static class CustomExceptions
    {
        public const string MaxResultSizeException = "Request exceeds maximum record size per instance of ";
    }

    public static class SubType
    {
        public const string Parent = "Parent";
    }

    public static class InClauseConstants
    {
        public const string Headers = "Headers";
        public const string Query = "Query";
        public const string Child = "Child";
        public const string OData = "OData";
        public const string Body = "Body";
    }
}
