using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zurich.Connector.Data.Utils;

namespace Zurich.Connector.Data.Repositories
{
    /// <summary>
    /// Provides an implementation of IRedisRepository for managing objects in Redis cache.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the RedisRepository class.
    /// </remarks>
    /// <param name="cache">The distributed cache abstraction.</param>
    public class RedisRepository(IDistributedCache cache) : IRedisRepository
    {
        /// <summary>
        /// Asynchronously retrieves an object of type T from the cache using the specified key.
        /// </summary>
        /// <param name="key">The cache key used to retrieve the object.</param>
        /// <typeparam name="T">The type of object to retrieve. Must be a class.</typeparam>
        /// <returns>The object of type T retrieved from the cache, or null if not found.</returns>
        public async Task<T> GetAsync<T>(string key) where T : class
        {
            string base64String = await cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(base64String))
            {
                return null;
            }

            return DeserializeFromBase64String<T>(base64String);
        }

        /// <summary>
        /// Asynchronously stores an object of type T in the cache using the specified key.
        /// </summary>
        /// <param name="key">The cache key to store the object under.</param>
        /// <param name="data">The object of type T to store in the cache.</param>
        /// <typeparam name="T">The type of object to store. Must be a class.</typeparam>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SetAsync<T>(string key, T data) where T : class
        {
            DistributedCacheEntryOptions options = new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };

            await cache.SetStringAsync(key, GetBase64String(data), options);
        }

        /// <summary>
        /// Converts an object of type T to a base64-encoded string.
        /// </summary>
        /// <param name="data">The object to convert.</param>
        /// <typeparam name="T">The type of object to convert. Must be a class.</typeparam>
        /// <returns>A base64-encoded string representing the object.</returns>
        private static string GetBase64String<T>(T data) where T : class
        {
            string json = SerializerUtils.SerializeToJson(data);

            return Base64Utils.Encode(json);
        }

        /// <summary>
        /// Deserializes a base64-encoded string to an object of type T.
        /// </summary>
        /// <param name="base64String">The base64-encoded string to deserialize.</param>
        /// <typeparam name="T">The type of object to deserialize. Must be a class.</typeparam>
        /// <returns>The deserialized object of type T.</returns>
        private static T DeserializeFromBase64String<T>(string base64String)
        {
            string json = Base64Utils.Decode(base64String);

            return SerializerUtils.DeserializeFromJson<T>(json);
        }
    }
}
