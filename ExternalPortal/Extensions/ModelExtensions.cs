using ExternalPortal.Enums;
using ExternalPortal.ViewModels;
using ExternalPortal.ViewModels.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Ofgem.API.GGSS.Domain.Commands.Applications;
using Ofgem.API.GGSS.Domain.Enums;
using Ofgem.API.GGSS.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExternalPortal.Extensions
{
    public static class ModelExtensions
    {
        public static bool IsTaskStarted(this TaskListViewModel summary, TaskType taskType)
        {
            var inProgress = summary.StageOneTasks().Any(t => t.TaskType == taskType && t.Status == TaskStatus.InProgress);
            var complete = summary.StageOneTasks().Any(t => t.TaskType == taskType && t.Status == TaskStatus.Completed);
            return inProgress || complete;
        }
    }
}
