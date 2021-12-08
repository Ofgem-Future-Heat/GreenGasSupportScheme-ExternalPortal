using ExternalPortal.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using GovUkDesignSystem.Attributes.ValidationAttributes;

namespace ExternalPortal.ViewModels.Value
{
    public class OptionsViewModel : ValueViewModel
    {
        public List<SelectListItem> Options { get; set; }
        public SelectListItem Selected { get; set; }
        public string Heading { get; set; }
        public string Description { get; set; }
        public string GreenPanelText { get; set; }
        public string ControllerName { get; set; }
        public string SelectedValue { get; set; }
        public string Error { get; set; }

        public OptionsViewModel(TaskType taskType, TaskPropertyName propertyName) 
            : base(taskType, propertyName, typeof(string))
        {
            this.Options = new List<SelectListItem>();
        }

        public virtual void SetupOptions()
        {

        }
    }
}
