using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{

    using System;

    public enum CountsStatusCode
    {
        Began = 1,
        InProgress = 2,
        ReadSuccess = 3,
        Completed = 4,
        Error = 99
    }

    public class StartCountsPost
    {
        public string SourceSystem { get; set; }
        public string CorrelationId { get; set; }
        public string Details { get; set; }
        public DateTime? ClientOccurredAtUtc { get; set; }
    }

    public class CountsStatusPost
    {
        public Guid ProcessId { get; set; }
        public int StatusCode { get; set; }
        public string SourceSystem { get; set; }
        public string CorrelationId { get; set; }
        public string Details { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorStackTrace { get; set; }

        public DateTime? ClientOccurredAtUtc { get; set; } // optional client timestamp
    }
}

