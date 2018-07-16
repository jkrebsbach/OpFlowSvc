using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class FlowStep
    {
        public int StepID { get; set; }
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
        public int SurgeryID { get; set; }
        public string StepStatus { get; set; }
        public TimeSpan StepStartTime { get; set; }
        public TimeSpan StepEndTime { get; set; }
    }

    public class Step
    {
        public int StepID { get; set; }
        public string StepName { get; set; }
        public string StepNotificationType { get; set; }

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
    }

    public class StepPost
    {
        public string StepName { get; set; }
        public string StepNotificationType { get; set; }
    }
}
