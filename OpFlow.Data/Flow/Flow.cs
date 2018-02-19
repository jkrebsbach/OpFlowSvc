using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Flow
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
    }
}
