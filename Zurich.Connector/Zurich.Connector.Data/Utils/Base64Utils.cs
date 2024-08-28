using System;
using System.IO;
using System.Text;

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

        /// <summary>
        /// Converts a Base64 encoded string to a byte array.
        /// </summary>
        /// <param name="base64EncodedData">The Base64 encoded string to convert.</param>
        /// <returns>A byte array. If the input is null or empty, returns an empty byte array.</returns>
        public static byte[] DecodeToByteArray(string base64EncodedData)
        {
            if (string.IsNullOrEmpty(base64EncodedData))
            {
                return [];
            }

            return Convert.FromBase64String(base64EncodedData);
        }

        /// <summary>
        /// Converts a Base64 encoded string to a stream.
        /// </summary>
        /// <param name="base64EncodedData">The Base64 encoded string to convert.</param>
        /// <returns>A stream containing the decoded data. If the input is null or empty, returns an empty stream.</returns>
        public static Stream DecodeToStream(string base64EncodedData)
        {
            byte[] data = DecodeToByteArray(base64EncodedData);

            MemoryStream stream = new(data)
            {
                Position = 0
            };

            return stream;
        }

        /// <summary>
        /// Converts a stream to a Base64 encoded string.
        /// </summary>
        /// <param name="inputStream">The stream to convert.</param>
        /// <returns>A Base64 encoded string. If the input stream is null or empty, returns an empty string.</returns>
        public static string EncodeStreamToBase64(Stream inputStream)
        {
            if (inputStream == null || inputStream.Length == 0)
            {
                return string.Empty;
            }

            // Ensure the stream position is at the beginning
            if (inputStream.CanSeek)
            {
                inputStream.Position = 0;
            }

            using MemoryStream memoryStream = new();
            inputStream.CopyTo(memoryStream);
            byte[] inputBytes = memoryStream.ToArray();
            return Convert.ToBase64String(inputBytes);
        }
    }
}