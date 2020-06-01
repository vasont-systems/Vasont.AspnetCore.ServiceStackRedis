//-----------------------------------------------------------------------
// <copyright file="RedisDistributedCacheEntry.cs" company="GlobalLink Vasont">
// Copyright (c) GlobalLink Vasont. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Vasont.AspnetCore.ServiceStackRedis.Models
{
    using System;

    /// <summary>
    /// This class is used for representing cache item
    /// </summary>
    public class RedisDistributedCacheEntry<T> : RedisDistributedCacheEntry
    {
        /// <summary>
        /// Gets or sets value
        /// </summary>
        public T Value { get; set; }
    }

    public class RedisDistributedCacheEntry
    {
        /// <summary>
        /// Gets or sets sliding expiration value
        /// </summary>
        public TimeSpan? SlidingExpiration { get; set; }

        /// <summary>
        /// Gets or sets absolute expiration time
        /// </summary>
        public DateTimeOffset? AbsoluteExpiration { get; set; }

        /// <summary>
        /// Gets or sets last accessed date
        /// </summary>
        public DateTimeOffset? LastAccessed { get; set; }
    }
}
