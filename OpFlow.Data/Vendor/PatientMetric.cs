using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class ProcedureProfileMetric
    {
        public int ProcedureProfileMetricID { get; set; }
        public int? ParentMetricID { get; set; }
        public int? ParentMetricAnswerID { get; set; }
        public string QuestionType { get; set; }
        public string QuestionText { get; set; }
        public string MetricType { get; set; }
        public string ParentMetric { get; set; }
        public string ParentMetricAnswer { get; set; }

        public string QuestionTypeDescription
        {
            get
            {
                switch (QuestionType)
                {
                    case "TXT":
                        return "Text";
                    case "SEL":
                    default:
                        return "Multiple Choice";
                }
            }
        }

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
        public int? ParentMetricAnswerID { get; set; }
        public string MetricType { get; set; }
        public string QuestionType { get; set; }
        public string TextPayload { get; set; }
    }

    public class ProcedureProfileMetricXrefPost
    {
        public List<int> Answers { get; set; }
    }
}
