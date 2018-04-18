using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Debrief
{
    public class DebriefResult
    {
        public Flow Flow { get; set; }
        public List<SmartPhraseCategory> PhraseCategories { get; set; }
        public List<FlowPhrase> FlowPhrases { get; set; }
        public List<SurgeryPhrase> SurgeryPhrases { get; set; }
        public List<SmartPhrase> SmartPhrases { get; set; }
        public List<FlowFeedback> FlowFeedback { get; set; }
        public List<FlowSurgeonNote> SurgeonNotes { get; set; }
        public List<FlowStepTiming> FlowSteps { get; set; }
        public List<Messaging> Messages { get; set; }
        public List<FlowImage> FlowImages { get; set; }
    }

    public class SmartPhraseCategory
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
    }
}
