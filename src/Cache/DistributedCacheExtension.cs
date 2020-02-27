//-----------------------------------------------------------------------
// <copyright file="DistributedCacheExtension.cs" company="GlobalLink Vasont">
// Copyright (c) GlobalLink Vasont. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Vasont.AspnetCore.ServiceStackRedis.Cache
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using Newtonsoft.Json;

    /// <summary>
    /// This class is used for adding Distributed Cache according to configuration
    /// </summary>
    public static class DistributedCacheExtension
    {
        /// <summary>
        /// This method is used to get value by key from storage
        /// </summary>
        /// <typeparam name="T">Contains the type of content to retrieve.</typeparam>
        /// <param name="cache">Contains a cache storage</param>
        /// <param name="key">Contains a cache storage key</param>
        /// <returns>Returns a value by key</returns>
        public static T Get<T>(this IDistributedCache cache, string key)
        {
            T result;

            switch (cache) 
            {
                case RedisDistributedCache distributedCache:
                    result = distributedCache.Get<T>(key);
                    break;
                case MemoryDistributedCache memoryDistributedCache:
                    result = JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(memoryDistributedCache.Get(key)));
                    break;
                default:
                    throw new InvalidOperationException();
            }

            return result;
        }

        /// <summary>
        /// This method is used to get value by key from storage
        /// </summary>
        /// <typeparam name="T">Contains the type of content to retrieve.</typeparam>
        /// <param name="cache">Contains a cache storage</param>
        /// <param name="key">Contains a cache storage key</param>
        /// <param name="token">Contains a cancellation token.</param>
        /// <returns>Returns a value by key</returns>
        public static Task<T> GetAsync<T>(this IDistributedCache cache, string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(Get<T>(cache, key));
        }

        /// <summary>
        /// This method is used to set cache value by key
        /// </summary>
        /// <typeparam name="T">Contains the type of content to set.</typeparam>
        /// <param name="cache">Contains a cache storage</param>
        /// <param name="key">Contains a cache storage key</param>
        /// <param name="value">Contains a value to be cached</param>
        /// <param name="options">Contains options for caching</param>
        public static void Set<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options)
        {
            switch (cache) 
            {
                case RedisDistributedCache distributedCache:
                    distributedCache.Set<T>(key, value, options);
                    break;
                case MemoryDistributedCache memoryDistributedCache:
                    memoryDistributedCache.Set(key, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(value)), options);
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// This method is used to set cache value by key
        /// </summary>
        /// <typeparam name="T">Contains the type of content to set.</typeparam>
        /// <param name="cache">Contains a cache storage</param>
        /// <param name="key">Contains a cache storage key</param>
        /// <param name="value">Contains a value to be cached</param>
        /// <param name="options">Contains options for caching</param>
        /// <param name="token">Contains a cancellation token.</param>
        /// <returns>Returns the task to execute.</returns>
        public static Task SetAsync<T>(this IDistributedCache cache, string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return Task.Run(() => Set<T>(cache, key, value, options), token);
        }
    }
}
