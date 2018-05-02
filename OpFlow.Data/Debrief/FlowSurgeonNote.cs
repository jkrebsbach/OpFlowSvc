using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Debrief
{
    public class FlowSurgeonNote
    {
        public int FlowSurgeonNoteID { get; set; }
        public int? FlowStepID { get; set; }
        public int? RoleID { get; set; }
        public string FlowStep { get; set; }
        public string RoleName { get; set; }
        public string SurgeonComment { get; set; }

    }
}
