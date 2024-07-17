using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.IO;
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
        private static DistributedCacheEntryOptions Options
        {
            get
            {
                return new() { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30) };
            }
        }

        public async Task<Stream> GetAsync(string key)
        {
            byte[] data = await cache.GetAsync(key);

            if (data is null)
            {
                return default;
            }

            return new MemoryStream(data);
        }

        public async Task SetAsync(string key, Stream stream)
        {
            byte[] data;

            using (MemoryStream memoryStream = new())
            {
                await stream.CopyToAsync(memoryStream);
                data = memoryStream.ToArray();
            }

            await cache.SetAsync(key, data, Options);
        }

        public async Task SetAsStringAsync(string key, string data)
        {
            await cache.SetStringAsync(key, EncodeBase64(data), Options);
        }

        public async Task<string> GetAsStringAsync(string key)
        {
            string data = await cache.GetStringAsync(key);

            if (string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }

            return DecodeBase64<string>(data);
        }

        /// <summary>
        /// Encodes the specified data object to Base64 string.
        /// </summary>
        /// <typeparam name="T">The type of the data object.</typeparam>
        /// <param name="data">The data object to encode.</param>
        /// <returns>The Base64 encoded string.</returns>
        private static string EncodeBase64<T>(T data) where T : class
        {
            string content = SerializerUtils.SerializeToJson(data);

            return Base64Utils.Encode(content);
        }

        /// <summary>
        /// Decodes the specified Base64 string to the original data object.
        /// </summary>
        /// <typeparam name="T">The type of the data object.</typeparam>
        /// <param name="base64String">The Base64 encoded string.</param>
        /// <returns>The decoded data object.</returns>
        private static string DecodeBase64<T>(string base64String) where T : class
        {
            string encodedContent = Base64Utils.Decode(base64String);

            return SerializerUtils.DeserializeFromJson<string>(encodedContent);
        }
    }
}
