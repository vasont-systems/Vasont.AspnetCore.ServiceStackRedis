//-----------------------------------------------------------------------
// <copyright file="RedisDistributedCache.cs" company="GlobalLink Vasont">
// Copyright (c) GlobalLink Vasont. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Vasont.AspnetCore.ServiceStackRedis.Cache
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Caching.Distributed;
    using ServiceStack.Redis;
    using ServiceStack.Redis.Generic;

    /// <summary>
    /// This class implements <see cref="IDistributedCache"/> interface with using Redis server
    /// </summary>
    public class RedisDistributedCache : IDistributedCache
    {
        #region Private Readonly Properties

        /// <summary>
        /// Contains a Redis Manager
        /// </summary>
        private readonly IRedisClientsManager redisManager;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisDistributedCache"/> class.
        /// </summary>
        /// <param name="manager">Contains a Redis Client Manager</param>
        public RedisDistributedCache(IRedisClientsManager manager)
        {
            this.redisManager = manager;
        }

        #endregion

        #region Methods Implementation

        /// <summary>
        /// This method is used to get value by key from storage
        /// </summary>
        /// <param name="key">Contains a cache storage key</param>
        /// <returns>Returns a value by key</returns>
        public byte[] Get(string key)
        {
            byte[] result = null;

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            using (var client = this.redisManager.GetClient() as IRedisNativeClient)
            {
                if (client.Exists(key) == 1)
                {
                    result = client.Get(key);
                }
            }

            return result;
        }

        /// <summary>
        /// This method is used to get value by key from storage
        /// </summary>
        /// <typeparam name="T">Contains the type of content to retrieve.</typeparam>
        /// <param name="key">Contains a cache storage key</param>
        /// <returns>Returns a value by key</returns>
        public T Get<T>(string key)
        {
            T result = default;

            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            using (var client = this.redisManager.GetClient())
            {
                if (client.ContainsKey(key))
                {
                    result = client.Get<T>(key);
                }
            }

            return result;
        }

        /// <summary>
        /// This method is used to get value asynchronously from the cache storage
        /// </summary>
        /// <param name="key">Contains a cache storage key</param>
        /// <param name="token">Contains a cancellation token</param>
        /// <returns>Returns a task with value result</returns>
        public Task<byte[]> GetAsync(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(this.Get(key));
        }

        /// <summary>
        /// This method is used to get value asynchronously from the cache storage
        /// </summary>
        /// <typeparam name="T">Contains the type of content to retrieve.</typeparam>
        /// <param name="key">Contains a cache storage key</param>
        /// <param name="token">Contains a cancellation token</param>
        /// <returns>Returns a task with value result</returns>
        public Task<T> GetAsync<T>(string key, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.FromResult(this.Get<T>(key));
        }

        /// <summary>
        /// This method is used to set cache value by key
        /// </summary>
        /// <param name="key">Contains a cache storage key</param>
        /// <param name="value">Contains a value to be cached</param>
        /// <param name="options">Contains options for caching</param>
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
        {
            this.Set<byte[]>(key, value, options);
        }

        /// <summary>
        /// This method is used to set cache value by key
        /// </summary>
        /// <typeparam name="T">Contains the type of content to set.</typeparam>
        /// <param name="key">Contains a cache storage key</param>
        /// <param name="value">Contains a value to be cached</param>
        /// <param name="options">Contains options for caching</param>
        public void Set<T>(string key, T value, DistributedCacheEntryOptions options)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            using (var client = this.redisManager.GetClient())
            {
                var expireInSeconds = this.GetExpireInSeconds(options);
                
                if (expireInSeconds > 0)
                {
                    client.Set(key, value, TimeSpan.FromSeconds(expireInSeconds));
                }
                else
                {
                    client.Set(key, value);
                }
            }
        }

        /// <summary>
        /// This method is used to cache value by key asynchronously
        /// </summary>
        /// <param name="key">Contains a cache storage key</param>
        /// <param name="value">Contains a value to be cached</param>
        /// <param name="options">Contains options for caching</param>
        /// <param name="token">Contains a cancellation token</param>
        /// <returns>Returns the task to execute.</returns>
        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            return Task.Run(() => this.Set(key, value, options), token);
        }

        /// <summary>
        /// This method is used to cache value by key asynchronously
        /// </summary>
        /// <typeparam name="T">Contains the type of content to set.</typeparam>
        /// <param name="key">Contains a cache storage key</param>
        /// <param name="value">Contains a value to be cached</param>
        /// <param name="options">Contains options for caching</param>
        /// <param name="token">Contains a cancellation token</param>
        /// <returns>Returns the task to execute.</returns>
        public Task SetAsync<T>(string key, T value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            return Task.Run(() => this.Set(key, value, options), token);
        }

        /// <summary>
        /// This method is used to refresh the value by key
        /// </summary>
        /// <param name="key">Contains a cache storage key</param>
        public void Refresh(string key)
        {
            // TODO: Implement Sliding expiration
        }

        /// <summary>
        /// This method is used to refresh the value by key asynchronously
        /// </summary>
        /// <param name="key">Contains a cache storage key</param>
        /// <param name="token">Contains a cancellation token.</param>
        /// <returns>Returns the task to execute.</returns>
        public Task RefreshAsync(string key, CancellationToken token = default)
        {
            return Task.Run(() => this.Refresh(key), token);
        }

        /// <summary>
        /// This method is used to remove cache entry by key
        /// </summary>
        /// <param name="key">Contains a cache storage key</param>
        public void Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                throw new ArgumentNullException(nameof(key));
            }

            using (var client = this.redisManager.GetClient())
            {
                client.Remove(key);
            }
        }

        /// <summary>
        /// This method is used to remove cache entry by key asynchronously
        /// </summary>
        /// <param name="key">Contains a cache storage key</param>
        /// <param name="token">Contains a cancellation token</param>
        /// <returns>Returns the task to execute.</returns>
        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            return Task.Run(() => this.Remove(key), token);
        }


        /// <summary>
        /// This method is used for generation expiration key
        /// </summary>
        /// <param name="pattern">Contains a key pattern</param>
        /// <param name="pageSize">Contains a page size for result</param>
        /// <returns>Returns the list of keys by pattern and page size</returns>
        public IEnumerable<string> FindKeys(string pattern, int pageSize = 1000)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            using (var client = this.redisManager.GetClient())
            {
                return client.ScanAllKeys(pattern, pageSize);
            }
        }

        /// <summary>
        /// This method is used for generation expiration key
        /// </summary>
        /// <param name="key">Contains a key</param>
        /// <returns>Returns the value indicating is key exists in storage</returns>
        public bool ContainsKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            using (var client = this.redisManager.GetClient())
            {
                return client.ContainsKey(key);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This method is used to get expiration value in seconds
        /// </summary>
        /// <param name="options">Contains the distributed cache options.</param>
        /// <returns>Returns the expiration in seconds.</returns>
        private int GetExpireInSeconds(DistributedCacheEntryOptions options)
        {
            int result = 0;

            if (options.SlidingExpiration.HasValue)
            {
                result = (int)options.SlidingExpiration.Value.TotalSeconds;
            }
            else if (options.AbsoluteExpiration.HasValue)
            {
                result = (int)(options.AbsoluteExpiration.Value - DateTimeOffset.Now).TotalSeconds;
            }
            else if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                result = (int)options.AbsoluteExpirationRelativeToNow.Value.TotalSeconds;
            }

            return result;
        }

        #endregion
    }
}
