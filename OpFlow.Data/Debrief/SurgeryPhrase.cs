using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Debrief
{
    public class SurgeryPhrase : SmartPhrase
    {
        public int SurgeryID { get; set; }
        public int FlowStepID { get; set; }
        public int RoleID { get; set; }
        public int UserID { get; set; }
        public string PhraseComment { get; set; }
        public string Status { get; set; }
    }
}
