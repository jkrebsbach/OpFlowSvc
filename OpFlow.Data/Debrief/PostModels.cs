using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Debrief
{
    public class SmartPhrasePost
    {
        public string Phrase { get; set; }
        public int CategoryID { get; set; }
        public int RoleID { get; set; }
        public int StepID { get; set; }
    }

    public class SurgeonNotePost
    {
        public string Phrase { get; set; }
        public int FlowID { get; set; }
        public int RoleID { get; set; }
        public int StepID { get; set; }
    }

    public class FlowFeedbackPost
    {
        public string Feedback { get; set; }
    }

    public class DebriefUpdatePost
    {
        public string CaseNotes { get; set; }
        public List<FlowPhrase> SelectedPhrases { get; set; }
    }
}
