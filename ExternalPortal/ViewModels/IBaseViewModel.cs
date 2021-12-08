using System.Collections.Generic;
using System.Linq;

namespace ExternalPortal.ViewModels
{
    public interface IBaseViewModel
    {
        IList<string> Errors { get; }
        public bool HasErrors => (this.Errors != null && this.Errors.Any());
        void AddError(string error);
    }
}
