using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Flow : IBindableEntity
    {
        public int FlowID { get; set; }
        public int CardID { get; set; }
        public int OwnerUserID { get; set; }
        public int TemplateFlowID { get; set; }
        public int TemplateRoomID { get; set; }
        public int SpecialtyID { get; set; }
        public string FlowDescription { get; set; }
        public string CardDescription { get; set; }
        public string OwnerLastName { get; set; }
        public string OwnerFirstName { get; set; }
        public int TotalMinutes { get; set; }
        public int AvgMinutes { get; set; }
        public int TimesUsed { get; set; }
        public decimal TimeCost { get; set; }
        public decimal CardCost { get; set; }
        public decimal LocationCost { get; set; }

        public decimal TotalCost => TimeCost + CardCost;

        public int GetID()
        {
            return FlowID;
        }

        public override string ToString()
        {
            return FlowDescription;
        }
    }

    public class SurgeryFlow : Flow
    {
        public bool CurrentFlow { get; set; }
        public decimal CurrentCostDelta { get; set; }
    }

    public class FlowDetail
    {
        public Flow Flow { get; set; }
        public List<FlowStepTiming> Steps { get; set; }
        public List<FlowFeedback> Feedback { get; set; }
        public List<FlowNotification> Notifications { get; set; }
        public List<FlowStepInstructionResult> Instructions { get; set; }
        public List<FlowContent> Content { get; set; }
    }

    public class FlowPost
    {
        public int CardID { get; set; }
        public int RoomSetupID { get; set; }
        public string FlowDescription { get; set; }
    }

    public class FlowStepPost
    {
        public List<FlowStepDetailPost> FlowSteps { get; set; }
    }

    public class FlowStepDetailPost
    {
        public int StepDuration { get; set; }
        public string StepDescription { get; set; }
    }

    public class FlowNotificationPost
    {
        public int StepID { get; set; }
        public int NotificationType { get; set; }
        public string Message { get; set; }
        public string SmsNumber { get; set; }
        public string EmailAddress { get; set; }
        public int? MessagingUserID { get; set; }
    }
}
