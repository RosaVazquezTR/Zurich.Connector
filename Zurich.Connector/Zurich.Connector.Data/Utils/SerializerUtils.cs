using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Utf8Json;

namespace Zurich.Connector.Data.Utils
{
    /// <summary>
    /// A static SerializerUtils class that provides functionality for serializing and deserializing objects.
    /// </summary>
    public static class SerializerUtils
    {
        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <param name="obj">The object to serialize.</param>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <returns>A string representing the serialized object in JSON format.</returns>
        public static string SerializeToJson<T>(T obj)
        {
            return JsonSerializer.ToJsonString(obj);
        }

        /// <summary>
        /// Deserializes a JSON string to an object of the specified type.
        /// </summary>
        /// <param name="jsonString">The JSON string to deserialize.</param>
        /// <typeparam name="T">The type of the object to which the JSON string is deserialized.</typeparam>
        /// <returns>An object of the specified type, constructed from the JSON string.</returns>
        public static T DeserializeFromJson<T>(string jsonString)
        {
            return JsonSerializer.Deserialize<T>(jsonString);
        }
    }
}
