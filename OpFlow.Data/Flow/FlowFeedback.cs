using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class FlowFeedback
    {
        public int FlowStepFeedbackID { get; set; }
        public int FlowID { get; set; }
        public int StepID { get; set; }
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string FeedbackType { get; set; }
        public string Feedback { get; set; }
    }
}
