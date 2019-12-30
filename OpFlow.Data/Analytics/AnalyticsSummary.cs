using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace OpFlow.Data
{
    public class ReportGroup
    {
        public string Category { get; set; }
        public List<AnalyticsSummary> Reports { get; set; }
    }
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
        public string CardName { get; set; }
        public string Category { get; set; }
        public string Instrument { get; set; }
        public decimal QtyOpen { get; set; }
        public int TrayCases { get; set; }
    }

    public class AnalyticsConcordance
    {
        public string InstrumentName { get; set; }
        public string SurgeonName { get; set; }
        public decimal QtyOpen { get; set; }
    }

    public class AnalyticsConcordanceResult
    {
        public List<string> Instruments { get; set; }
        public List<AnalyticsConcordanceSurgon> Surgeons { get; set; }

        public AnalyticsConcordanceResult()
        {
            Instruments = new List<string>();
            Surgeons = new List<AnalyticsConcordanceSurgon>();
        }
    }

    public class AnalyticsConcordanceSurgon
    {
        public string SurgeonName { get; set; }
        public List<decimal> QtyOpen { get; set; }
    }

    public class ConcordanceReportSummary
    {
        public string TrayName { get; set; }
        public string UsageSummary {
            get {
                var usageRatio = 0.0M;
                if (TrayItems.Sum(ti => ti.Quantity) != 0)
                    usageRatio = (TrayItems.Sum(ti => ti.Usage) / TrayItems.Sum(ti => ti.Quantity));
                return $"{usageRatio.ToString("P", CultureInfo.InvariantCulture)} ({TrayItems.Sum(ti => ti.Usage).ToString("0.##")}/{TrayItems.Sum(ti => ti.Quantity).ToString("0.##")})";
            }
        }

        public List<ConcordanceItem> TrayItems { get; set; }

        public ConcordanceReportSummary()
        {
            TrayItems = new List<ConcordanceItem>();
        }
    }
    public class ConcordanceItem
    {
        public decimal Quantity { get; set; }
        public decimal Usage { get; set; }
    }

    public class InstrumentUsagePost
    {
        public List<int> ProposedTrayID { get; set; }
        public List<int> SpecialtyID { get; set; }
        public List<int> SurgeonID { get; set; }
        public List<int> CategoryID { get; set; }
        public List<int> ProcedureID { get; set; }
        public List<int> TrayID { get; set; }
        public List<int> CardCategoryID { get; set; }
        public List<int> CardID { get; set; }
        public List<int> ItemID { get; set; }
        public List<int> InstrumentID { get; set; }
        public List<int> TrayPhaseID { get; set; }
        public List<string> Cpt { get; set; }
        public List<string> TrayStatus { get; set; }
        public string Instruments { get; set; }
        public int? MinCost { get; set; }
        public int? MinQty { get; set; }
        public decimal? MinOpen { get; set; }
        public decimal? MinHold { get; set; }
        public int? Redundancy { get; set; }
        public int? CaseProfileId { get; set; }
        public List<int> QuestionId { get; set; }
        public List<int> AnswerId { get; set; }
        public bool FieldAll { get; set; }
        public bool FieldWaste { get; set; }
        public bool FieldOver { get; set; }
        public bool FieldUnder { get; set; }
        public string Order { get; set; }
        public string Group { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class TrayRationalizationReportPost
    {
        public List<int> SpecialtyId { get; set; }
        public List<int> SurgeonId { get; set; }
        public List<int> TrayId { get; set; }
        public List<int> CardCategoryId { get; set; }
        public int? MinSize { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CaseProfileId { get; set; }
        public List<int> QuestionId { get; set; }
        public List<int> AnswerId { get; set; }
        public string Order { get; set; }
        public string Group { get; set; }

        public bool FieldCase { get; set; }
        public bool FieldInstrument { get; set; }
        public bool FieldUsage { get; set; }
        public bool FieldTray { get; set; }
    }

    public class TrayConsolidationReportPost
    {
        public List<int> SpecialtyId { get; set; }
        public List<int> TrayId { get; set; }
        public List<int> ProcedureGroup { get; set; }
        public int? Reallocation { get; set; }
        public int? Overlap { get; set; }
        public int? MaxSize { get; set; }
        public int? MinCards { get; set; }
        public int? MinConsolidationInstances { get; set; }
        public int? MinTargetInstances { get; set; }
        public int? Effect { get; set; }
        public string Group { get; set; }
    }

    public class TrayConsolidationExportPost
    {
        public List<TrayConsolidationExport> Exports { get; set; }
    }

    public class TrayConsolidationExport
    {
        public int TrayID { get; set; }
        public List<TrayConsolidationExport> Children { get; set; }
    }

    public class CountSummaryReportPost
    {
        public List<int> SpecialtyId { get; set; }
        public List<int> SurgeonId { get; set; }
        public List<int> CardId { get; set; }
        public List<int> CardCategoryId { get; set; }
        public List<int> RoomGroupId { get; set; }
        public string Order { get; set; }
    }

    public class ServiceLineSummarySpecialty
    {
        public string Specialty { get; set; }
        public int CardQuantity => Results.Sum(r => r.CardQuantity);
        public decimal ItemCost => Results.Sum(r => r.ItemCost);
        public int CaseCount => Results.Sum(r => r.CaseCount);
        public List<ServiceLineSummary> Results { get; set; }
    }

    public class ServiceLineSummary
    {
        public string ItemName { get; set; }
        public int SpecialtyID { get; set; }
        public string Specialty { get; set; }
        public string Surgeon { get; set; }
        public string CardName { get; set; }
        public string ProcedureName { get; set; }
        public int CardQuantity { get; set; }
        public decimal ItemCost { get; set; }
        public decimal AvgSetup { get; set; }
        public decimal AvgOpen { get; set; }
        public decimal AvgUsageVariance { get; set; }
        public decimal AvgCardQtyVariance { get; set; }
        public decimal AvgUsed { get; set; }
        public int WasteAmount { get; set; }
        public int OverAllocation { get; set; }
        public int UnderAllocation { get; set; }
        public int CaseCount { get; set; }

        public string Waste => (WasteAmount > 0) ? "Y" : "N";
        public string Allocation => (OverAllocation > 0) ? "OVER" : 
            (UnderAllocation < 0 ? "UNDER" : "");
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
