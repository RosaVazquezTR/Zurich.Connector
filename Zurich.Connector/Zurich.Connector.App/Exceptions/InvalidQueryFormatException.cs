using System;

namespace Zurich.Connector.App.Exceptions
{
    /// <summary>
    /// Invlid query format exception
    /// </summary>
    public class InvalidQueryFormatException : Exception
    {
        public InvalidQueryFormatException(string message) : base(message)
        {

        }
    }
}
