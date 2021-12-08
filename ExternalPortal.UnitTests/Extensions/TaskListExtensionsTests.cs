using ExternalPortal.Extensions;
using ExternalPortal.ViewModels;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ExternalPortal.UnitTests.Extensions
{
    public class TaskListExtensionsTests
    {
        [Fact]
        public void GetSectionsCompleted()
        {
            // Arrange 
            var taskListViewModel = new TaskListViewModel();

            // Add Stage One tasks
            taskListViewModel.Tasks.AddRange(new List<TaskItemViewModel>
            {
                new TaskItemViewModel(Enums.TaskType.PlantDetails)
                {
                    Status = Enums.TaskStatus.Completed,
                },
                new TaskItemViewModel(Enums.TaskType.PlanningPermission)
                {
                    Status = Enums.TaskStatus.Completed,
                },
                new TaskItemViewModel(Enums.TaskType.ProductionDetails)
                {
                    Status = Enums.TaskStatus.NotStarted,
                },
                new TaskItemViewModel(Enums.TaskType.FeedstockDetails)
                {
                    Status = Enums.TaskStatus.NotStarted,
                }
            });

            // Act
            var sectionsCompleted = taskListViewModel.SectionsCompleted();

            // Assert
            sectionsCompleted.Should().Be(2);
        }

        [Fact]
        public void GetTotalSections()
        {
            // Arrange 
            var taskListViewModel = new TaskListViewModel();

            // Add Stage One tasks
            taskListViewModel.Tasks.AddRange(new List<TaskItemViewModel>
            {
                new TaskItemViewModel(Enums.TaskType.PlantDetails)
                {
                    Status = Enums.TaskStatus.Completed,
                },
                new TaskItemViewModel(Enums.TaskType.PlanningPermission)
                {
                    Status = Enums.TaskStatus.Completed,
                },
                new TaskItemViewModel(Enums.TaskType.ProductionDetails)
                {
                    Status = Enums.TaskStatus.NotStarted,
                },
                new TaskItemViewModel(Enums.TaskType.FeedstockDetails)
                {
                    Status = Enums.TaskStatus.NotStarted,
                }
            });

            // Add Stage Two tasks
            taskListViewModel.Tasks.AddRange(new List<TaskItemViewModel> {
                new TaskItemViewModel(Enums.TaskType.Isae3000)
                {
                    Status = Enums.TaskStatus.CannotStartYet,
                },
                new TaskItemViewModel(Enums.TaskType.SupportingEvidence)
                {
                    Status = Enums.TaskStatus.CannotStartYet,
                }
            });

            // Add Stage Three tasks
            taskListViewModel.Tasks.AddRange(new List<TaskItemViewModel> {
                new TaskItemViewModel(Enums.TaskType.NetworkEntryAgreement)
                {
                    Status = Enums.TaskStatus.CannotStartYet,
                },
                new TaskItemViewModel(Enums.TaskType.CommissioningEvidence)
                {
                    Status = Enums.TaskStatus.CannotStartYet,
                },
                new TaskItemViewModel(Enums.TaskType.BiogasProductionPlant)
                {
                    Status = Enums.TaskStatus.CannotStartYet,
                },
                new TaskItemViewModel(Enums.TaskType.FuelMeasurementAndSamplingQuestionaire)
                {
                    Status = Enums.TaskStatus.CannotStartYet,
                },
                new TaskItemViewModel(Enums.TaskType.MeteringDetails)
                {
                    Status = Enums.TaskStatus.CannotStartYet,
                },
            });

            // Act
            var totalSections = taskListViewModel.TotalSections();

            // Assert
            totalSections.Should().Be(11);
        }
    }
}
