using System;
using System.Collections.Generic;
using System.Linq;
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

        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
    }

    public class FlowStepResult
    {
        public int FlowID { get; set; }
        public int? StepID { get; set; }
        public int? StepDuration { get; set; }
        public int? PreviousStepID { get; set; }
    }

    public class FlowStepTiming : FlowStep
    {
        public int? AverageDuration { get; set; }
    }

    public class FlowStepSurgeryTiming : FlowStepTiming
    {
        public string StepStatus { get; set; }
        public bool PatientIn { get; set; }
        public bool PatientOut { get; set; }
        public TimeSpan EstStartTime { get; set; }
        public TimeSpan ScheduleTime { get; set; }
        public DateTime ScheduleDate { get; set; }


        public DateTime EstStartDateTime => ScheduleDate.Add(EstStartTime);

        public static void CalculateEstimatedTimes(List<FlowStepSurgeryTiming> timings)
        {
            var startTime = timings.FirstOrDefault()?.StartTime ?? timings.FirstOrDefault()?.ScheduleTime;
            if (startTime == null)
                return;

            var currTime = startTime.Value;
            foreach (var t in timings)
            {
                t.EstStartTime = currTime;
                currTime = currTime.Add(new TimeSpan(0, t.StepDuration, 0));
            }
        }
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
