using ExternalPortal.Enums;
using ExternalPortal.ViewModels;
using Ofgem.Azure.Redis.Data.Contracts;
using System;
using System.Collections.Generic;

namespace ExternalPortal.Models.Redis
{
    /// <summary>
    /// Files size cannot exceed 512Mb
    /// </summary>
    public class ApplicationFiles : IAzureRedisStoreEntity
    {
        /// <summary>
        /// ApplicationId
        /// </summary>
        public Guid Id { get; set; }
        public Dictionary<TaskPropertyName, TempFileUpload> Files { get; set; }
        public ApplicationFiles()
        {
            this.Files = new Dictionary<TaskPropertyName, TempFileUpload>();
        }
    }
}
