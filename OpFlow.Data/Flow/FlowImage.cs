using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class FlowImage
    {
        public int FlowImageID { get; set; }
        public string FlowImagePath { get; set; }
        public int FlowStepID { get; set; }
        public int RoleID { get; set; }
    }
}
