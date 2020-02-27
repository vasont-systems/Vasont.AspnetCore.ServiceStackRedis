//-----------------------------------------------------------------------
// <copyright file="ServiceStackRedisOptions.cs" company="GlobalLink Vasont">
// Copyright (c) GlobalLink Vasont. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Vasont.AspnetCore.ServiceStackRedis.Models
{
    /// <summary>
    /// Contains the service stack redis options.
    /// </summary>
    public class ServiceStackRedisOptions
    {
        /// <summary>
        /// Gets or sets Redis connection string
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets maximum pool size for connections
        /// </summary>
        public int MaxPoolSize { get; set; }
    }
}
