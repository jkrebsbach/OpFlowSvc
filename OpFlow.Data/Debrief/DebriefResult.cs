using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Debrief
{
    public class DebriefResult
    {
        public List<SmartPhraseCategory> Categories { get; set; }
        public List<FlowFeedback> FlowFeedback { get; set; }
        public List<FlowSurgeonNote> SurgeonNotes { get; set; }
    }

    public class SmartPhraseCategory
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public List<FlowPhrase> FlowPhrases { get; set; }

        public SmartPhraseCategory()
        {
            FlowPhrases = new List<FlowPhrase>();
        }
    }
}
