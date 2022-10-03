using System;

namespace Zurich.Connector.App.Exceptions
{
    /// <summary>
    /// Custom required parameter missing exception
    /// </summary>
    public class RequiredParameterMissingException : Exception
    {
        public RequiredParameterMissingException(string message) : base(message)
        {

        }
    }
}
