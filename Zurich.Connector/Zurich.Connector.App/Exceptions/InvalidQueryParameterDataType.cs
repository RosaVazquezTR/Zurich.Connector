using System;

namespace Zurich.Connector.App.Exceptions
{
    /// <summary>
    /// Exception for query parameter with invalid data type
    /// </summary>
    public class InvalidQueryParameterDataType : Exception
    {
        public InvalidQueryParameterDataType(string message) : base(message)
        {

        }
    }
}