using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class FlowInstruction : FlowStep
    {
        public int SmartPhraseID { get; set; }
        public int RoleID { get; set; }
        public decimal AverageDuration { get; set; }
        public string StepInstruction { get; set; }
        public string RoleDescription { get; set; }
    }

    public class FlowStepInstructionResult
    {
        public int StepID { get; set; }
        public string StepName { get; set; }
        public List<FlowRoleInstruction> FlowRoleInstructions { get; }

        public FlowStepInstructionResult()
        {
            FlowRoleInstructions = new List<FlowRoleInstruction>();
        }
    }

    public class FlowImageResult
    {
        public List<FlowImage> FlowImages { get; set; }
        public List<SurgeryImage> SurgeryImages { get; set; }
    }

    public class FlowRoleInstruction
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public List<FlowInstruction> FlowInstructions { get; }

        public FlowRoleInstruction()
        {
            FlowInstructions = new List<FlowInstruction>();
        }
    }
}
