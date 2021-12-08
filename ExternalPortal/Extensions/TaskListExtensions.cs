using ExternalPortal.ViewModels;
using System.Linq;

namespace ExternalPortal.Extensions
{
    public static class TaskListExtensions
    {
        public static int SectionsCompleted(this TaskListViewModel taskListViewModel)
        {
            var sectionsCompleted = taskListViewModel.StageOneTasks().Count(t => t.Status == Enums.TaskStatus.Completed);

            sectionsCompleted += taskListViewModel.StageTwoTasks().Count(t => t.Status == Enums.TaskStatus.Completed);

            sectionsCompleted += taskListViewModel.StageThreeTasks().Count(t => t.Status == Enums.TaskStatus.Completed);

            return sectionsCompleted;
        }

        public static int TotalSections(this TaskListViewModel taskListViewModel)
        {
            int totalSections = taskListViewModel.StageOneTasks().Count;
            totalSections += taskListViewModel.StageTwoTasks().Count;
            totalSections += taskListViewModel.StageThreeTasks().Count;
            return totalSections;
        }
    }
}
