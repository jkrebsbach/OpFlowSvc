using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class FlowInstruction : FlowStep
    {
        public int RoleID { get; set; }
        public decimal AverageDuration { get; set; }
        public string StepInstruction { get; set; }
        public string RoleDescription { get; set; }
    }
}
