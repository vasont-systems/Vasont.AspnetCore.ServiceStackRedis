//-----------------------------------------------------------------------
// <copyright file="RedisDistributedCacheExtensions.cs" company="GlobalLink Vasont">
// Copyright (c) GlobalLink Vasont. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Vasont.AspnetCore.ServiceStackRedis.Cache
{
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// This class is used for initializing Service Stack Redis distributed cache
    /// </summary>
    public static class RedisDistributedCacheExtensions
    {
        /// <summary>
        /// This method is used to initialize Service Stack Redis distributed cache
        /// </summary>
        /// <param name="services">Contains a service collection</param>
        public static void AddDistributedServiceStackRedisCache(this IServiceCollection services)
        {
            services.AddSingleton<IDistributedCache, RedisDistributedCache>();
        }
    }
}
