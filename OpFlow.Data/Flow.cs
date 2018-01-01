using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Flow
    {
        public int FlowID { get; set; }
        public int CardID { get; set; }
        public int UserID { get; set; }
        public string Description { get; set; }
        public int TotalMinutes { get; set; }
        public int AvgMinutes { get; set; }
        public int TimesUsed { get; set; }

        public List<FlowStep> FlowSteps { get; set; }
        public List<FlowMetric> FlowMetrics { get; set; }
    }
}
