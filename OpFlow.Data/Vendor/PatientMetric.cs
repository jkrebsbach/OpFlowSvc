using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class PatientMetric
    {
        public int PatientMetricID { get; set; }
        public string QuestionText { get; set; }

        public List<PatientMetricAnswer> Answers { get; set; }
    }
    public class PatientMetricAnswer
    {
        public int PatientMetricAnswerID { get; set; }
        public int PatientMetricID { get; set; }
        public string AnswerText { get; set; }
    }

    public class PatientMetricPost
    {
        public string TextPayload { get; set; }
    }

    public class PatientMetricProfilePost
    {
        public List<int> Answers { get; set; }
    }
}
