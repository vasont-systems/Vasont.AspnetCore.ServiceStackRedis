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
    using Vasont.AspnetCore.ServiceStackRedis.Models;

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
            return this.Get<byte[]>(key);
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
                    var entry = client.Get<RedisDistributedCacheEntry<T>>(key);

                    if (entry != null)
                    {
                        result = entry.Value;
                    }

                    this.Refresh(key);
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

            var creationTime = DateTimeOffset.UtcNow;

            var absoluteExpiration = this.GetAbsoluteExpiration(creationTime, options);

            using (var client = this.redisManager.GetClient())
            {
                var entry = new RedisDistributedCacheEntry<T>
                            {
                                    Value = value,
                                    AbsoluteExpiration = absoluteExpiration?.Ticks,
                                    SlidingExpiration = options.SlidingExpiration?.Ticks
                            };

                var expiresIn = this.GetExpirationInSeconds(creationTime, absoluteExpiration, options);

                if (expiresIn.HasValue)
                {
                    client.Set(key, entry, TimeSpan.FromSeconds(expiresIn.Value));
                }
                else
                {
                    client.Set(key, entry);
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
            using (var client = this.redisManager.GetClient())
            {
                if (client.ContainsKey(key))
                {
                    var entry = client.Get<RedisDistributedCacheEntry>(key);

                    this.MapMetadata(entry, out DateTimeOffset? absoluteExpiration, out TimeSpan? slidingExpiration);

                    // Note Refresh has no effect if there is just an absolute expiration (or neither).
                    if (slidingExpiration.HasValue)
                    {
                        TimeSpan expiration;

                        if (absoluteExpiration.HasValue)
                        {
                            var relativeExpiration = absoluteExpiration.Value - DateTimeOffset.UtcNow;
                            expiration = relativeExpiration <= slidingExpiration.Value ? relativeExpiration : slidingExpiration.Value;
                        }
                        else
                        {
                            expiration = slidingExpiration.Value;
                        }

                        client.ExpireEntryIn(key, expiration);
                    }
                }
            }
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

        #region Private methods

        /// <summary>
        /// This method is used to get the absolute expiration date time offset.
        /// </summary>
        /// <param name="creationTime">Contains the creation time.</param>
        /// <param name="options">Contains cache entry options.</param>
        /// <returns>Returns the absolute expiration date time offset.</returns>
        private DateTimeOffset? GetAbsoluteExpiration(DateTimeOffset creationTime, DistributedCacheEntryOptions options)
        {
            if (options.AbsoluteExpiration.HasValue && options.AbsoluteExpiration <= creationTime)
            {
                throw new ArgumentOutOfRangeException(nameof(DistributedCacheEntryOptions.AbsoluteExpiration), options.AbsoluteExpiration.Value, "Expiration Date must be in the future");
            }

            var absoluteExpiration = options.AbsoluteExpiration;

            if (options.AbsoluteExpirationRelativeToNow.HasValue)
            {
                absoluteExpiration = creationTime + options.AbsoluteExpirationRelativeToNow;
            }

            return absoluteExpiration;
        }

        /// <summary>
        /// This method is used to get expiration in seconds.
        /// </summary>
        /// <param name="creationTime">Contains the creation time.</param>
        /// <param name="absoluteExpiration">Contains the absolute expiration date.</param>
        /// <param name="options">Contains cache entry options.</param>
        /// <returns>Returns the number of seconds before expiration.</returns>
        private double? GetExpirationInSeconds(DateTimeOffset creationTime, DateTimeOffset? absoluteExpiration, DistributedCacheEntryOptions options)
        {
            double? result = null;

            if (absoluteExpiration.HasValue && options.SlidingExpiration.HasValue)
            {
                result = Math.Min((absoluteExpiration.Value - creationTime).TotalSeconds, options.SlidingExpiration.Value.TotalSeconds);
            }
            else if (absoluteExpiration.HasValue)
            {
                result = (absoluteExpiration.Value - creationTime).TotalSeconds;
            }
            else if (options.SlidingExpiration.HasValue)
            {
                result = options.SlidingExpiration.Value.TotalSeconds;
            }

            return result;
        }

        /// <summary>
        /// This method is used to map metadata.
        /// </summary>
        /// <param name="entry">Contains the redis entry value.</param>
        /// <param name="absoluteExpiration">Contains an absolute expiration value.</param>
        /// <param name="slidingExpiration">Contains a sliding expiration value.</param>
        private void MapMetadata(RedisDistributedCacheEntry entry, out DateTimeOffset? absoluteExpiration, out TimeSpan? slidingExpiration)
        {
            absoluteExpiration = null;
            slidingExpiration = null;

            var absoluteExpirationTicks = entry.AbsoluteExpiration;

            if (absoluteExpirationTicks.HasValue)
            {
                absoluteExpiration = new DateTimeOffset(absoluteExpirationTicks.Value, TimeSpan.Zero);
            }

            var slidingExpirationTicks = entry.SlidingExpiration;

            if (slidingExpirationTicks.HasValue)
            {
                slidingExpiration = new TimeSpan(slidingExpirationTicks.Value);
            }
        }

        #endregion
    }
}
