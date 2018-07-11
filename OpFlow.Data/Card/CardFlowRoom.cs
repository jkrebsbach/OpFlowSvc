using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class CardFlowRoom
    {
        public int? BundleID { get; set; }
        public int CptCount { get; set; }
        public int ProcedureCount { get; set; }
        public int CardID { get; set; }
        public int? ProcedureID { get; set; }
        public int TemplateFlowID { get; set; }
        public int TemplateRoomSetupID { get; set; }

        public string CardDescription { get; set; }
        public string FlowDescription { get; set; }
        public string RoomDescription { get; set; }
    }
}
