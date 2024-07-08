using System;
using System.Collections.Generic;
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
        /// Asynchronously stores an object of type T in the Redis cache using the specified key.
        /// </summary>
        /// <param name="key">The key under which the object is to be stored in the cache.</param>
        /// <param name="data">The object of type T to store in the cache.</param>
        /// <typeparam name="T">The type of the object to store. Must be a class.</typeparam>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SetAsync<T>(string key, T data) where T : class;

        /// <summary>
        /// Asynchronously retrieves an object of type T from the Redis cache using the specified key.
        /// </summary>
        /// <param name="key">The key used to retrieve the object from the cache.</param>
        /// <typeparam name="T">The type of the object to retrieve. Must be a class.</typeparam>
        /// <returns>A task that, when completed successfully, returns the retrieved object of type T. If the object is not found, returns null.</returns>
        Task<T> GetAsync<T>(string key) where T : class;
    }
}
