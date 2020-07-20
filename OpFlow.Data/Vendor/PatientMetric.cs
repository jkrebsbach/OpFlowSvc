using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class ProcedureProfileMetric
    {
        public int ProcedureProfileMetricID { get; set; }
        public string QuestionText { get; set; }
        public string MetricType { get; set; }

        public List<ProcedureProfileMetricAnswer> Answers { get; set; }
    }
    public class ProcedureProfileMetricAnswer
    {
        public int ProcedureProfileMetricAnswerID { get; set; }
        public int ProcedureProfileMetricID { get; set; }
        public string AnswerText { get; set; }
    }

    public class ProcedureProfileMetricPost
    {
        public string MetricType { get; set; }
        public string TextPayload { get; set; }
    }

    public class ProcedureProfileMetricXrefPost
    {
        public List<int> Answers { get; set; }
    }
}
