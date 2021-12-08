using System.ComponentModel.DataAnnotations;

namespace ExternalPortal.Enums
{
    public enum TaskStatus
    {
        [Display(Name = "Cannot Start Yet")]
        CannotStartYet,

        [Display(Name = "Not Started")] 
        NotStarted,

        [Display(Name = "In Progress")] 
        InProgress,

        Completed,

        Submitted,

        [Display(Name = "In Review")]
        InReview,

        [Display(Name = "With Applicant")]
        WithApplicant,

        Approved
    }
}
