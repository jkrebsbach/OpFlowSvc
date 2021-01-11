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
        public string QuestionOwner { get; set; }
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
                    case "MANY":
                        return "Choose Many";
                    case "SEL":
                    default:
                        return "Multiple Choice";
                }
            }
        }
        public string QuestionOwnerDescription
        {
            get
            {
                switch (QuestionOwner)
                {
                    case "OPF":
                        return "OpFlow Rep";
                    case "SUR":
                        return "Surgeon";
                    case "CIR":
                        return "Circulator";
                    case "SCR":
                        return "Scrub";
                    case "VEN":
                    default:
                        return "Vendor Rep";
                }
            }
        }

        public List<ProcedureProfileMetricAnswer> Answers { get; set; }
        public List<ProcedureProfileMetricCardCategory> CardCategories { get; set; }
    }
    public class ProcedureProfileMetricAnswer
    {
        public int ProcedureProfileMetricAnswerID { get; set; }
        public int ProcedureProfileMetricID { get; set; }
        public string AnswerText { get; set; }
    }
    public class ProcedureProfileMetricCardCategory
    {
        public int ProcedureProfileMetricID { get; set; }
        public int CardCategoryID { get; set; }
    }

    public class ProcedureProfileMetricPost
    {
        public int? ParentMetricAnswerID { get; set; }
        public string MetricType { get; set; }
        public string QuestionOwner { get; set; }
        public string QuestionType { get; set; }
        public string TextPayload { get; set; }
    }

    public class ProcedureProfileMetricXrefPost
    {
        public List<int> Answers { get; set; }
    }
}
