using Microsoft.AspNetCore.Identity;
using Ofgem.Azure.Redis.Data.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace ExternalPortal.ViewModels
{
    public class PortalViewModel<TModel> : IBaseViewModel, IAzureRedisStoreEntity where TModel : class, new()
    {
        /// <summary>
        /// This is the key for Redis store
        /// </summary>
        public Guid Id { get; set; }

        public TModel Model { get; set; }

        public IList<string> Errors { get; private set; }

        public bool HasErrors => this.Errors.Any();
        public Dictionary<Type,Guid> RedisKeys { get; set; }
        public List<TempFileUpload> Uploads { get; set; }

        public PortalViewModel()
        {
            this.Errors = new List<string>();
            this.RedisKeys = new Dictionary<Type, Guid>();
            this.Uploads = new List<TempFileUpload>();
        }

        public PortalViewModel(TModel model) : this()
        {
            this.Model = model ?? throw new ArgumentNullException(nameof(model));
        }

        public void AddError(string error)
        {
            if (string.IsNullOrWhiteSpace(error)) return;
            this.Errors.Add(error);
        }
        
        public string ReturnUrl { get; set; }
    }
}
