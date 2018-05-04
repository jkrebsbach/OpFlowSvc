using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class SurgeryImage
    {
        public int SurgeryImageID { get; set; }
        public string SurgeryImagePath { get; set; }
        public int FlowStepID { get; set; }
        public int RoleID { get; set; }
        public string FlowStep { get; set; }
        public string RoleName { get; set; }
        public string ImageComment { get; set; }
    }
}
