using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class FlowStep
    {
        public int StepID { get; set; }
        public int FlowID { get; set; }
        public int RoleID { get; set; }
        public int SurgeryID { get; set; }
        public string FlowStepStatus { get; set; }
        public string StepDescription { get; set; }
        public int StepDuration { get; set; }
        public string StepInstruction { get; set; }
    }
}
