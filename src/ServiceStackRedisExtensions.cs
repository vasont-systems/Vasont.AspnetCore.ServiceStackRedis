//-----------------------------------------------------------------------
// <copyright file="ServiceStackRedisExtensions.cs" company="GlobalLink Vasont">
// Copyright (c) GlobalLink Vasont. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Vasont.AspnetCore.ServiceStackRedis
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using ServiceStack.Redis;
    using Vasont.AspnetCore.ServiceStackRedis.Models;

    /// <summary>
    /// This class is used for adding Distributed Cache according to configuration
    /// </summary>
    public static class ServiceStackRedisExtensions
    {
        /// <summary>
        /// This method is used to initialize Service Stack Redis distributed cache
        /// </summary>
        /// <param name="services">Contains a service collection</param>
        /// <param name="section">Contains a configuration section</param>
        public static void AddServiceStackRedis(this IServiceCollection services, IConfigurationSection section)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            services.AddOptions();
            services.Configure<ServiceStackRedisOptions>(section);
            ServiceStackRedisOptions options = section.Get<ServiceStackRedisOptions>();

            AddServiceStackRedis(services, options);
        }

        /// <summary>
        /// This method is used to initialize Service Stack Redis distributed cache
        /// </summary>
        /// <param name="services">Contains a service collection</param>
        /// <param name="options">Contains the service stack options used to configure the singlton redis clients manager instance.</param>
        public static void AddServiceStackRedis(this IServiceCollection services, ServiceStackRedisOptions options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            if (!string.IsNullOrEmpty(options.ConnectionString))
            {
                services.AddSingleton<IRedisClientsManager>(c => 
                    new RedisManagerPool(
                        options.ConnectionString,
                        new RedisPoolConfig
                        {
                            MaxPoolSize = options.MaxPoolSize
                        }));
            }
        }
    }
}
