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

    public class AnalyticsInstrumentUsage
    {
        public string TrayName { get; set; }
        public string Category { get; set; }
        public string Instrument { get; set; }
        public decimal QtyOpen { get; set; }
        public int TrayCases { get; set; }
    }

    public class InstrumentUsagePost
    {
        public int? SpecialtyID { get; set; }
        public int? SurgeonID { get; set; }
        public int? CategoryID { get; set; }
        public int? ProcedureID { get; set; }
        public int? TrayID { get; set; }
        public List<string> Cpt { get; set; }
    }

    public class InstrumentUsageSummaryResult
    {
        public string TrayName { get; set; }
        public int RowSize { get; set; }
        public decimal InstrumentCount { get; set; }
        public int CaseCount { get; set; }
        public List<string> Instruments { get; set; }
        public List<decimal> QtyOpen { get; set; }
    }
}
