using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Utils
{
    /// <summary>
    /// Provides methods to encode and decode strings using Base64 encoding.
    /// </summary>
    public static class Base64Utils
    {
        /// <summary>
        /// Encodes a plain text string into a Base64 string.
        /// </summary>
        /// <param name="plainText">The plain text string to encode.</param>
        /// <returns>A Base64 encoded string. If the input is null or empty, returns an empty string.</returns>
        public static string Encode(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
            {
                return string.Empty;
            }

            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        /// <summary>
        /// Decodes a Base64 encoded string back into a plain text string.
        /// </summary>
        /// <param name="base64EncodedData">The Base64 encoded string to decode.</param>
        /// <returns>A plain text string. If the input is null or empty, returns an empty string.</returns>
        public static string Decode(string base64EncodedData)
        {
            if (string.IsNullOrEmpty(base64EncodedData))
            {
                return string.Empty;
            }

            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}