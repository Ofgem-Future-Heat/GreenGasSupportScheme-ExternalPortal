using Ofgem.Azure.Redis.Data.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Ofgem.API.GGSS.Domain.Enums;

namespace ExternalPortal.ViewModels
{
    public class TaskListViewModel : IAzureRedisStoreEntity
    {
        public Guid Id { get; set; }

        public Guid ApplicationId { get; set; }

        public List<TaskItemViewModel> Tasks { get; private set; }

        public bool StageOneApproved { get; set; }

        public bool StageTwoApproved { get; set; }

        //WithApplicant is added to account for legacy data
        public bool StageOneInProgress => Status == ApplicationStatus.Draft.ToString() || Status == ApplicationStatus.StageOneWithApplicant.ToString() || Status == "WithApplicant";
        
        public bool StageTwoInProgress => Status == ApplicationStatus.StageOneApproved.ToString() || Status == ApplicationStatus.StageTwoWithApplicant.ToString();
        
        public bool StageTwoEditable =>
            StageOneApproved || Status == ApplicationStatus.StageTwoWithApplicant.ToString();
        
        public string Status { get; set; }
        
        public bool CanSubmit { get; set; }
        public bool StageOneCanSubmit => IsAuthorisedSignatoryAndAllStageOneTasksComplete();
        public bool StageTwoCanSubmit => IsAuthorisedSignatoryAndAllStageTwoTasksComplete();
        
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
        
        private bool IsAuthorisedSignatoryAndAllStageOneTasksComplete()
        {
            return CanSubmit && StageOneTasks().All(t => t.Status == Enums.TaskStatus.Completed);
        }
        
        private bool IsAuthorisedSignatoryAndAllStageTwoTasksComplete()
        {
            return CanSubmit && StageTwoTasks().All(t => t.Status == Enums.TaskStatus.Completed);
        }

        public string GetSubmitText()
        {
            if (IsAuthorisedSignatoryAndAllStageOneTasksComplete() || IsAuthorisedSignatoryAndAllStageTwoTasksComplete())
            {
                return
                    "By clicking 'accept and submit', you're confirming that, to the best of your knowledge, the details you're providing are correct.";
            }
            
            return CanSubmit 
                ? "You cannot submit until you have completed all the tasks in this section." 
                : "You can only 'accept and send' if you're the authorised signatory.";
        }
    }
}
