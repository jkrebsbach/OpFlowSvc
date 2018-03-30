using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Debrief
{
    public class SmartPhrasePost
    {
        public string Category { get; set; }
        public string Phrase { get; set; }
    }

    public class FlowFeedbackPost
    {
        public string Feedback { get; set; }
    }

    public class DebriefUpdatePost
    {
        public List<FlowPhrase> SelectedPhrases { get; set; }
    }
}
