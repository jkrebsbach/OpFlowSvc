using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Debrief
{
    public class DebriefResult
    {
        public List<PatientPosition> PatientPositions { get; set; }
        public List<SmartPhrase> SmartPhrases { get; set; }
        public List<FlowPhrase> FlowPhrase { get; set; }
        public List<DebriefCategory> Categories { get; set; }
    }

    public class DebriefCategory
    {
        public string CategoryName { get; set; }
    }
}
