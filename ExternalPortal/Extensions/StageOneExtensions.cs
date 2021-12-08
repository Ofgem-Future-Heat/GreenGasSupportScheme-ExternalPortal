using System;
using System.Linq;
using ExternalPortal.Constants;
using Ofgem.API.GGSS.Domain.Enums;
using Ofgem.API.GGSS.Domain.Models;

namespace ExternalPortal.Extensions
{
    public static class StageOneExtensions
    {
        public static int GetStageOneOutstandingTasks(this ApplicationModel application)
        {
            if (application is null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            var outstandingTasks = 0;

            if (!application.IsStageOneTaskOneCompleted()) outstandingTasks++;
            if (!application.IsStageOneTaskTwoCompleted()) outstandingTasks++;
            if (!application.IsStageOneTaskThreeCompleted()) outstandingTasks++;
            if (!application.IsStageOneTaskFourCompleted()) outstandingTasks++;

            return outstandingTasks;
        }

        public static bool IsStageOneTaskOneCompleted(this ApplicationModel stageOne)
        {
            if (stageOne is null)
            {
                throw new ArgumentNullException(nameof(stageOne));
            }

            if (stageOne.Value is null)
            {
                throw new InvalidOperationException(nameof(stageOne.Value));
            }

            var taskCompleted = false;

            var step1 = false;
            var step2 = false;
            var step3 = false;
            var step4 = false;

            if (stageOne.Value.Location == Location.England ||
                stageOne.Value.Location == Location.Scotland ||
                stageOne.Value.Location == Location.Wales)
            {
                step1 = true;
            }

            if (stageOne.Documents.Any(t => t.Value.Tags == DocumentTags.CapacityCheck))
            {
                step2 = true;
            }

            if (!string.IsNullOrEmpty(stageOne.Value.InstallationName))
            {
                step3 = true;
            }


            if (!string.IsNullOrEmpty(stageOne.Value.InstallationAddress.LineOne) &&
                !string.IsNullOrEmpty(stageOne.Value.InstallationAddress.Town) &&
                !string.IsNullOrEmpty(stageOne.Value.InstallationAddress.Postcode))
            {
                step4 = true;
            }

            if (step1 && step2 && step3 && step4)
            {
                taskCompleted = true;
            }

            return taskCompleted;
        }

        public static bool IsStageOneTaskTwoCompleted(this ApplicationModel stageOne)
        {
            if (stageOne is null)
            {
                throw new ArgumentNullException(nameof(stageOne));
            }

            if (stageOne.Value is null)
            {
                throw new InvalidOperationException(nameof(stageOne.Value));
            }

            return stageOne.Documents.Any(t => t.Value.Tags == DocumentTags.PlanningPermission) ||
                   stageOne.Documents.Any(t => t.Value.Tags == DocumentTags.PlanningExempt); 
        }

        public static bool IsStageOneTaskThreeCompleted(this ApplicationModel stageOne)
        {
            if (stageOne is null)
            {
                throw new ArgumentNullException(nameof(stageOne));
            }

            if (stageOne.Value is null)
            {
                throw new InvalidOperationException(nameof(stageOne.Value));
            }

            var taskCompleted = false;

            var step1 = false;
            var step2 = false;

            if (!stageOne.Value.MaxCapacity.Equals(null))
            {
                step1 = true;
            }

            if (!stageOne.Value.DateInjectionStart.Equals(DateTime.MinValue))
            {
                step2 = true;
            }

            if (step1 && step2)
            {
                taskCompleted = true;
            }

            return taskCompleted;
        }

        public static bool IsStageOneTaskFourCompleted(this ApplicationModel stageOne)
        {
            if (stageOne is null)
            {
                throw new ArgumentNullException(nameof(stageOne));
            }

            if (stageOne.Value is null)
            {
                throw new InvalidOperationException(nameof(stageOne.Value));
            }

            return false;
        }
    }
}
