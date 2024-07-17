using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Zurich.Connector.Data.Repositories
{
    /// <summary>
    /// Defines a contract for a repository that handles asynchronous operations for storing and retrieving objects from Redis cache.
    /// </summary>
    public interface IRedisRepository
    {
        /// <summary>
        /// Asynchronously stores binary data in the Redis cache using the specified key.
        /// </summary>
        /// <param name="key">The key under which the binary data is to be stored in the cache.</param>
        /// <param name="stream">The binary data to store in the cache.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetAsync(string key, Stream stream);

        /// <summary>
        /// Asynchronously stores a string value in the Redis cache using the specified key.
        /// </summary>
        /// <param name="key">The key under which the string value is to be stored in the cache.</param>
        /// <param name="data">The string value to store in the cache.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetAsStringAsync(string key, string data);

        /// <summary>
        /// Asynchronously retrieves binary data from the Redis cache using the specified key.
        /// </summary>
        /// <param name="key">The key used to retrieve the binary data from the cache.</param>
        /// <returns>A task that, when completed successfully, returns the retrieved binary data as a Stream. If the data is not found, returns null.</returns>
        Task<Stream> GetAsync(string key);

        /// <summary>
        /// Asynchronously retrieves a string value from the Redis cache using the specified key.
        /// </summary>
        /// <param name="key">The key used to retrieve the string value from the cache.</param>
        /// <returns>A task that, when completed successfully, returns the retrieved string value. If the value is not found, returns null.</returns>
        Task<string> GetAsStringAsync(string key);
    }
}
