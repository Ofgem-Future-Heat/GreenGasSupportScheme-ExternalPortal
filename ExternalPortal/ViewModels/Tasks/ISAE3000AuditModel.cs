using System.Collections.Generic;
using ExternalPortal.Enums;

namespace ExternalPortal.ViewModels.Tasks
{
    public class Isae3000AuditModel : IApplicationTaskItemModel
    {
        public Isae3000AuditModel()
        {
            State = TaskStatus.NotStarted;
        }
        public TaskType Type => TaskType.Isae3000;
        public TaskStatus State { get; set; }
        public string Filename { get; set; }
        public string DocumentId { get; set; }
        public string FileSizeAsString { get; set; }
        public List<Option> Options => GetOptions();
        public string Error { get; set; }

        public void Start()
        {
            State = TaskStatus.InProgress;
        }

        public void Finish()
        {
            State = TaskStatus.Completed;
        }

        public bool HasError()
        {
            return !string.IsNullOrEmpty(Error);
        }

        private List<Option> GetOptions()
        {
            return new List<Option>()
            {
                new Option(){ Id = "1", Text = "Yes, add this file", Value = "Yes"},
                new Option(){ Id = "2", Text = "No, I want to choose a different file", Value = "No"}
            };
        }
    }

    public class Option
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public string Value { get; set; }
    }
}