using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Debrief
{
    public class DebriefResult
    {
        public List<RoomSetup> RoomSetups { get; set; }
        public List<SmartPhrase> SmartPhrases { get; set; }
        public List<DebriefCategory> Categories { get; set; }
        public List<FlowFeedback> FlowFeedback { get; set; }
    }

    public class DebriefCategory
    {
        public string CategoryName { get; set; }

        public List<FlowPhrase> FlowPhrases { get; set; }

        public DebriefCategory()
        {
            FlowPhrases = new List<FlowPhrase>();
        }
    }
}
