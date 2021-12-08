using ExternalPortal.Enums;
using GovUkDesignSystem.Attributes.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace ExternalPortal.ViewModels.Value
{
    public class StringViewModel : ValueViewModel
    {
        public string Heading { get; set; }
        public string Description { get; set; }
        
        [GovUkValidateRequired(ErrorMessageIfMissing = "Enter feedstock supplier's name")]
        public string Value { get; set; }

        public TaskValueStatus Status
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.Value)) return TaskValueStatus.NotStarted;
                return TaskValueStatus.Completed;
            }
        }
        public StringViewModel(TaskType taskType, TaskPropertyName propertyName)
            : base(taskType, propertyName, typeof(string))
        {

        }
        public StringViewModel() :base()
        {

        }
    }
}
