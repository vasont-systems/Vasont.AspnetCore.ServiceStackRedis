﻿//-----------------------------------------------------------------------
// <copyright file="RedisXmlRepository.cs" company="GlobalLink Vasont">
// Copyright (c) GlobalLink Vasont. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
namespace Vasont.AspnetCore.ServiceStackRedis.Storage
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.AspNetCore.DataProtection.Repositories;
    using ServiceStack.Redis;

    /// <summary>
    /// This class is used to implement <see cref="IXmlRepository"/> to store XML
    /// </summary>
    public class RedisXmlRepository : IXmlRepository
    {
        #region Private Properties
        /// <summary>
        /// Contains a unique identity for the repository.
        /// </summary>
        private readonly string listId = Guid.NewGuid().ToString("N");

        /// <summary>
        /// Contains a redis clients manager instance.
        /// </summary>
        private readonly IRedisClientsManager clientsManager;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisXmlRepository"/> class.
        /// </summary>
        /// <param name="clientsManager">Contains a Redis client manager</param>
        public RedisXmlRepository(IRedisClientsManager clientsManager)
        {
            this.clientsManager = clientsManager;
        }

        /// <summary>
        /// This method is used for getting all elements from the storage
        /// </summary>
        /// <returns>Returns a collection of elements</returns>
        public IReadOnlyCollection<XElement> GetAllElements()
        {
            using (var client = this.clientsManager.GetClient())
            {
                return client.GetAllItemsFromList(this.listId).Select(XElement.Parse).ToList();
            }
        }

        /// <summary>
        /// This method is used to store element
        /// </summary>
        /// <param name="element">Contains an element to be stored</param>
        /// <param name="friendlyName">Contains a friendly name</param>
        public void StoreElement(XElement element, string friendlyName)
        {
            using (var client = this.clientsManager.GetClient())
            {
                client.AddItemToList(this.listId, element.ToString(SaveOptions.DisableFormatting));
            }
        }
    }
}
