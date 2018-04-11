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
        public string OwnerLastName { get; set; }
        public int TotalMinutes { get; set; }
        public int AvgMinutes { get; set; }
        public int TimesUsed { get; set; }

        public int GetID()
        {
            return FlowID;
        }

        public override string ToString()
        {
            return FlowDescription;
        }
    }

    public class FlowDetail
    {
        public Flow Flow { get; set; }
        public List<FlowFeedback> Feedback { get; set; }
        public List<FlowNotification> Notifications { get; set; }
        public List<FlowStepInstructionResult> Instructions { get; set; }
        public List<FlowContent> Content { get; set; }
    }

}
