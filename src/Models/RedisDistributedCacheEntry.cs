//-----------------------------------------------------------------------
// <copyright file="RedisDistributedCacheEntry.cs" company="GlobalLink Vasont">
// Copyright (c) GlobalLink Vasont. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Vasont.AspnetCore.ServiceStackRedis.Models
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This class is used for representing cache item with value
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleType", Justification = "Reviewed.")]
    public class RedisDistributedCacheEntry<T> : RedisDistributedCacheEntry
    {
        /// <summary>
        /// Gets or sets value
        /// </summary>
        public T Value { get; set; }
    }

    /// <summary>
    /// This class is used for representing cache item
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleType", Justification = "Reviewed.")]
    public class RedisDistributedCacheEntry
    {
        /// <summary>
        /// Gets or sets sliding expiration value
        /// </summary>
        public long? SlidingExpiration { get; set; }

        /// <summary>
        /// Gets or sets absolute expiration time
        /// </summary>
        public long? AbsoluteExpiration { get; set; }
    }
}
