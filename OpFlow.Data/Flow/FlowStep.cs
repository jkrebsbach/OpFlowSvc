using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class FlowStep : Step
    {
        public int FlowID { get; set; }
        public int SurgeryID { get; set; }
        public int StepOwnerUserID { get; set; }
        public string FlowStepStatus { get; set; }
        public string StepDescription { get; set; }
        public int StepDuration { get; set; }
        public int StepSequence { get; set; }

        public List<FlowRoleInstruction> RoleInstructions { get; set; }
        public List<FlowNotification> StepNotifications { get; set; }
        public List<FlowImage> FlowImages { get; set; }
        public List<SurgeryImage> SurgeryImages { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class FlowStepTiming : FlowStep
    {
        public int? AverageDuration { get; set; }
    }

    public class FlowStepSurgeryTiming : FlowStepTiming
    {
        public string StepStatus { get; set; }
        public TimeSpan StepStartTime { get; set; }
        public TimeSpan StepEndTime { get; set; }
    }

    public class Step : IBindableEntity
    {
        public int StepID { get; set; }
        public string StepName { get; set; }
        public string StepNotificationType { get; set; }
        public bool DashboardTime { get; set; }

        public string StepNotificationDescription
        {
            get
            {
                switch (StepNotificationType)
                {
                    case "M":
                        return "Mobile Alert";
                    case "D":
                        return "Dashboard Only";
                    default:
                        return "UNKNOWN";
                }
            }
        }
        public string StepTimingDescription => DashboardTime ? "Include in timing" : "Exclude from timing";

        public int GetID()
        {
            return StepID;
        }

        public override string ToString()
        {
            return StepName;
        }
    }

    public class StepPost
    {
        public string StepName { get; set; }
        public string StepNotificationType { get; set; }
        public bool StepTiming { get; set; }
    }
}
