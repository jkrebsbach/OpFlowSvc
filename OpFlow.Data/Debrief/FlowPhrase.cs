using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Debrief
{
    public class FlowPhrase : SmartPhrase
    {
        public int FlowID { get; set; }
        public int FlowStepID { get; set; }
        public int RoleID { get; set; }
        public string FlowStep { get; set; }
        public string RoleName { get; set; }
        public int UserID { get; set; }
        public string PhraseComment { get; set; }
    }
}
