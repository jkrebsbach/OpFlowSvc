using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class AnalyticsSummary
    {
        public int TableID { get; set; }
        public string TableName { get; set; }
        public string Category { get; set; }
        public int RoleID { get; set; }
        public string PowerBILink { get; set; }
        public DateTime InsTimestamp { get; set; }
    }

    public class AnalyticTrayRationalization
    {
        public string TrayName { get; set; }
        public int CaseCount { get; set; }
        public int InstrumentCount { get; set; }
        public int UsageQuantity { get; set; }
        public int TrayOpened { get; set; }
    }

    public class AnalyticsCountSummary
    {
        public string Card { get; set; }
        public string Specialty { get; set; }
        public int TrayCount { get; set; }
        public int CardCount { get; set; }
    }
}
