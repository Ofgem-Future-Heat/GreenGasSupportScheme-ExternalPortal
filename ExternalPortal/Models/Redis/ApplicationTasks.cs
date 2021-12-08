using ExternalPortal.Enums;
using Ofgem.Azure.Redis.Data.Contracts;
using System;
using System.Collections.Generic;

namespace ExternalPortal.Models.Redis
{
    public class ApplicationTasks : IAzureRedisStoreEntity
    {
        /// <summary>
        /// Application Id
        /// </summary>
        public Guid Id { get; set; }
        public Dictionary<TaskType, TaskStatus> CurrentTasks { get; set; }

        public ApplicationTasks()
        {
            this.CurrentTasks = new Dictionary<TaskType, TaskStatus>();
        }
    }
}
