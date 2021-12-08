using ExternalPortal.Enums;
using System;
using System.Collections.Generic;

namespace ExternalPortal.ViewModels
{
    public class CheckAnswersViewModel
    {
        public Guid OrganisationId { get; set; }
        public TaskType TaskType { get; set; }
        public string BackAction { get; set; }
        public List<Answer> Answers { get; set; }

        public CheckAnswersViewModel(TaskType taskType)
        {
            this.TaskType = taskType;
            this.Answers = new List<Answer>();
        }
    }

    public struct Answer
    {
        public TaskPropertyName PropertyName { get; set; }
        public string PropertyValue { get; set; }
        public string ChangeLink { get; set; }
    }
}
