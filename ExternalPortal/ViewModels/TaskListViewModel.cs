using Ofgem.Azure.Redis.Data.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExternalPortal.ViewModels
{
    public class TaskListViewModel : IAzureRedisStoreEntity
    {
        public Guid Id { get; set; }

        public Guid ApplicationId { get; set; }

        public List<TaskItemViewModel> Tasks { get; private set; }

        public bool StageOneComplete => StageOneTasks().All(t => t.Status == Enums.TaskStatus.Completed);

        public bool StageTwoComplete
            => this.StageOneApproved && this.StageTwoTasks().All(t => t.Status == Enums.TaskStatus.Completed);

        public bool StageThreeComplete
            => this.StageTwoApproved && this.StageThreeTasks().All(t => t.Status == Enums.TaskStatus.Completed);

        public bool StageOneApproved { get; set; }

        public bool StageTwoApproved { get; set; }

        public int ApplicationProgress { get; set; }

        public TaskListViewModel() { this.Tasks = new List<TaskItemViewModel>(); }

        public List<TaskItemViewModel> StageOneTasks()
        {
            return GetTasksFor(Enums.ApplicationStage.StageOne);
        }

        public List<TaskItemViewModel> StageTwoTasks()
        {
            return GetTasksFor(Enums.ApplicationStage.StageTwo);
        }

        public List<TaskItemViewModel> StageThreeTasks()
        {
            return GetTasksFor(Enums.ApplicationStage.StageThree);
        }

        public void InitTaskItems()
        {
            // Add Stage One tasks
            this.Tasks.AddRange(new List<TaskItemViewModel>
            {
                new TaskItemViewModel(Enums.TaskType.PlantDetails)
                {
                    Stage = Enums.ApplicationStage.StageOne,
                    Status = Enums.TaskStatus.NotStarted,
                    StartActionName = "WhatYouWillNeed"
                },
                new TaskItemViewModel(Enums.TaskType.PlanningPermission)
                {
                    Stage = Enums.ApplicationStage.StageOne,
                    Status = Enums.TaskStatus.NotStarted,
                    StartActionName = "WhatYouWillNeed"
                },
                new TaskItemViewModel(Enums.TaskType.ProductionDetails)
                {
                    Stage = Enums.ApplicationStage.StageOne,
                    Status = Enums.TaskStatus.NotStarted,
                    StartActionName = "WhatYouWillNeed"
                }
            });

            // Add Stage Two tasks
            this.Tasks.AddRange(new List<TaskItemViewModel> {
                new TaskItemViewModel(Enums.TaskType.Isae3000)
                {
                    ControllerName = "Isae3000Audit",
                    Stage = Enums.ApplicationStage.StageTwo,
                    Status = Enums.TaskStatus.CannotStartYet,
                    StartActionName = "WhatYouWillNeed"
                },
                new TaskItemViewModel(Enums.TaskType.SupportingEvidence)
                {
                    Stage = Enums.ApplicationStage.StageTwo,
                    Status = Enums.TaskStatus.CannotStartYet,
                    StartActionName = "WhatYouWillNeed"
                }
            });

            // Add Stage Three tasks
            this.Tasks.AddRange(new List<TaskItemViewModel> {
                new TaskItemViewModel(Enums.TaskType.OrganisationDetails)
                {
                    Stage = Enums.ApplicationStage.StageThree,
                    Status = Enums.TaskStatus.CannotStartYet,
                    StartActionName = "WhatYouWillNeed"
                },
                new TaskItemViewModel(Enums.TaskType.NetworkEntryAgreement)
                {
                    Stage = Enums.ApplicationStage.StageThree,
                    Status = Enums.TaskStatus.CannotStartYet,
                    StartActionName = "WhatYouWillNeed"
                },
                new TaskItemViewModel(Enums.TaskType.CommissioningEvidence)
                {
                    Stage = Enums.ApplicationStage.StageThree,
                    Status = Enums.TaskStatus.CannotStartYet,
                    StartActionName = "WhatYouWillNeed"
                },
                new TaskItemViewModel(Enums.TaskType.FeedstockDetails)
                {
                    Stage = Enums.ApplicationStage.StageThree,
                    Status = Enums.TaskStatus.CannotStartYet,
                    StartActionName = "WhatYouWillNeed"
                },
                new TaskItemViewModel(Enums.TaskType.BiogasProductionPlant)
                {
                    Stage = Enums.ApplicationStage.StageThree,
                    Status = Enums.TaskStatus.CannotStartYet,
                    StartActionName = "WhatYouWillNeed"
                },
                new TaskItemViewModel(Enums.TaskType.EnvironmentalPermit)
                {
                    Stage = Enums.ApplicationStage.StageThree,
                    Status = Enums.TaskStatus.CannotStartYet,
                    StartActionName = "WhatYouWillNeed"
                },
                new TaskItemViewModel(Enums.TaskType.FuelMeasurementAndSamplingQuestionaire)
                {
                    Stage = Enums.ApplicationStage.StageThree,
                    Status = Enums.TaskStatus.CannotStartYet,
                    StartActionName = "WhatYouWillNeed"
                },
                new TaskItemViewModel(Enums.TaskType.MeteringDetails)
                {
                    Stage = Enums.ApplicationStage.StageThree,
                    Status = Enums.TaskStatus.CannotStartYet,
                    StartActionName = "WhatYouWillNeed"
                }
            });
        }

        private List<TaskItemViewModel> GetTasksFor(Enums.ApplicationStage stage)
        {
            return Tasks.Where(t => t.Stage == stage).ToList();
        }
    }
}
