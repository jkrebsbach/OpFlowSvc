using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class FlowMetric
    {
        public int FlowID { get; set; }
        public int StepID { get; set; }
        public int MetricType { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
    }
}
