using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Debrief
{
    public class SmartPhrasePost
    {
        public string Phrase { get; set; }
        public int? UserID { get; set; }
        public int CategoryID { get; set; }
        public int RoleID { get; set; }
        public int StepID { get; set; }
    }

    public class NewSmartPhrasePost : SmartPhrasePost
    {
        public int? FlowID { get; set; }
        public int? SurgeryID { get; set; }
    }

    public class SurgeonNotePost
    {
        public string Phrase { get; set; }
        public int FlowID { get; set; }
        public int RoleID { get; set; }
        public int StepID { get; set; }
    }

    public class FlowImagePost
    {
        public int FlowID { get; set; }
        public int RoleID { get; set; }
        public int StepID { get; set; }
        public string Comment { get; set; }
    }

    public class FlowFeedbackPost
    {
        public string Feedback { get; set; }
    }

    public class PhraseUpdatePost
    {
        public string Comments { get; set; }
        public int StepID { get; set; }
        public int RoleID { get; set; }
    }

    public class ChangeCptPost
    {
        public List<string> CptId { get; set; }
    }

    public class BatchEditModel
    {
        public List<int> IDList { get; set; }
    }
    public class SurgeryTrayFeedback
    {
        public string Feedback { get; set; }
    }
    public class DebriefUpdatePost
    {
        public string CaseNotes { get; set; }
    }
}
