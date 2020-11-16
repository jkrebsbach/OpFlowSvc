using OpFlow.Data.Analytics;
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

    public class CountDistributionOutput
    {
        public string CountType { get; set; }
        public string ProcedureGroup { get; set; }
        public string Specialty { get; set; }
        public string Card { get; set; }
        public string ItemName { get; set; }
        public string Surgeon { get; set; }
        public int CardCount { get; set; }
        public int? UnitCost { get; set; }
        public int CardQty { get; set; }
        public int Setup { get; set; }
        public int Usage { get; set; }
        public int Added { get; set; }
    }
    public class CountDistributionSummary
    {
        public int Supplies { get; set; }
        public int SurgeonCounts { get; set; }
        public int SurgeonNoCounts { get; set; }
        public int CardCounts { get; set; }
        public int CardNoCounts { get; set; }

        public List<CountDistributionReport> Data { get; set; }
    }
    public class CountDistributionReport
    {
        public string CountType { get; set; }
        public List<CountDistributionReportType> Data { get; set; }
    }
    public class CountDistributionReportType
    {
        public string ProcedureGroup { get; set; }
        public List<CountDistributionReportProcedureGroup> Data { get; set; }
    }
    public class CountDistributionReportProcedureGroup
    {
        public string ServiceLine { get; set; }
        public List<CountDistributionReportSpecialty> Data { get; set; }
    }
    public class CountDistributionReportSpecialty
    {
        public string Surgeon { get; set; }
        public List<CountDistributionReportSurgeon> Data { get; set; }

        public int SurgeonCount => Data.Sum(c => c.CardCount);
    }
    public class CountDistributionReportSurgeon
    {
        public string Card { get; set; }
        public List<CountDistributionReportCard> Data { get; set; }

        public int CardCount => Data.Max(d => d.CardCount);
    }
    public class CountDistributionReportCard
    {
        public string ItemName { get; set; }
        public int? UnitCost { get; set; }
        public int CardCount { get; set; }
        public int CardQty { get; set; }
        public int Setup { get; set; }
        public int Usage { get; set; }
        public int Added { get; set; }

        public decimal AvgOpen => CardCount == 0 ? 0 : (decimal)Setup / CardCount;
        public decimal AvgAdded => CardCount == 0 ? 0 : (decimal)Added / CardCount;
        public decimal AvgUsed => CardCount == 0 ? 0 : (decimal)Usage / CardCount;
    }

    public class ConcordanceReportSummary
    { 
        public List<ConcordanceReportTrayData> TrayData { get; set; }
        public List<ConcordanceReportItemData> Items { get; set; }
    }

    public class OpFlowProcedureProfileSummary
    {
        public int TotalSystems { get; set; }
        public int SurgeryCounts { get; set; }
        public int SurgeryAudits { get; set; }
        public int AvgDuration { get; set; }

        public List<OpFlowProcedureProfileSystemSummaryData> SystemSummaries { get; set; }
        public List<OpFlowProcedureProfileSummaryDetail> InternalTrays { get; set; }
        public List<OpFlowProcedureProfileSummaryDetail> VendorTrays { get; set; }
        public List<OpFlowProcedureProfileSummaryDetail> Items { get; set; }
        public List<Flow> Timings { get; set; }
    }

    public class OpFlowProcedureProfileSystemSummaryData
    {
        public string System { get; set; }
        public int TraysOpened { get; set; }
        public int InstrumentsUsed { get; set; }
        public int CaseDuration { get; set; }
        public int Turnover { get; set; }
        public int ProcedureDuration { get; set; }
    }

    public class OpFlowProcedureProfileSummaryData
    {
        public int SurgeonID { get; set; }
        public int LocationID { get; set; }
        public string ItemType { get; set; }
        public string ContainerName { get; set; }
        public string ItemName { get; set; }
        public int OPPQty { get; set; }
        public int LocationQty { get; set; }
        public int SurgeonCount { get; set; }
        public int SurgeonUsage { get; set; }
    }

    public class OpFlowProcedureProfileSummaryDetail
    {
        public string ItemType { get; set; }
        public string ContainerName { get; set; }
        public string ItemName { get; set; }
        public int OPPQty { get; set; }
        public int LocationQty { get; set; }
        public int? SurgeonQty { get; set; }
        public int SurgeonCount { get; set; }
        public decimal? OppUsage { get; set; }
        public decimal? LocationUsage { get; set; }
        public decimal? SurgeonUsage { get; set; }

        public string Reduction => (LocationUsage > OppUsage) ? "Y" : "";
    }

    public class ConcordanceReportItemData
    {
        public string ID { get; set; }
        public string InstrumentDescription { get; set; }
        public decimal QtyOpen { get; set; }
        public decimal TrayUsage { get; set; }
        public decimal TrayQty { get; set; }
    }

    public class ConcordanceReportTrayData
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

        public decimal ItemQuantity => TrayItems.Sum(t => t.Quantity);
        public decimal ItemUsage => TrayItems.Sum(t => t.Usage);

        public List<ConcordanceItem> TrayItems { get; set; }

        public ConcordanceReportTrayData()
        {
            TrayItems = new List<ConcordanceItem>();
        }
    }
    public class ConcordanceItem
    {
        public decimal Quantity { get; set; }
        public decimal Usage { get; set; }
    }

    public class ProcedureProfileReportPost: ReportRequest
    {
        public int? TrayTypeID { get; set; }
        public int ProcedureProfileID { get; set; }
        public int? SpecialtyID { get; set; }
        public List<int> ProcedureID { get; set; }
        public int? SurgeonID { get; set; }
        public int? MetricID { get; set; }
        public string Order { get; set; }
    }

    public class InstrumentUsagePost : ReportRequest
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
        public string VendorTray { get; set; }
        public int? TrayTypeID { get; set; }
        public int? MinCost { get; set; }
        public int? MinQty { get; set; }
        public decimal? MinOpen { get; set; }
        public decimal? MinHold { get; set; }
        public int? Redundancy { get; set; }
        public int? CaseProfileId { get; set; }
        public List<int> QuestionId { get; set; }
        public List<int> AnswerId { get; set; }
        public bool ShowMax { get; set; }
        public bool FieldAll { get; set; }
        public bool FieldWaste { get; set; }
        public bool FieldOver { get; set; }
        public bool FieldUnder { get; set; }
        public string Label { get; set; }
        public string Order { get; set; }
        public string Group { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class TrayRationalizationReportPost : ReportRequest
    {
        public List<int> SpecialtyId { get; set; }
        public List<int> SurgeonId { get; set; }
        public List<int> TrayId { get; set; }
        public List<int> CardCategoryId { get; set; }
        public List<int> CardId { get; set; }
        public int? MinSize { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? CaseProfileId { get; set; }
        public List<int> QuestionId { get; set; }
        public List<int> AnswerId { get; set; }
        public string VendorTray { get; set; }
        public int? TrayTypeId { get; set; }
        public string Order { get; set; }
        public string Group { get; set; }

        public bool FieldCase { get; set; }
        public bool FieldInstrument { get; set; }
        public bool FieldUsage { get; set; }
        public bool FieldTray { get; set; }
    }

    public class TrayConsolidationReportPost : ReportRequest
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

    public class SupplySavingsSummary
    {
        public decimal TotalCost { get; set; }
        public decimal WasteUnits { get; set; }
        public decimal WasteCost { get; set; }
        public decimal OverallocatedUnits { get; set; }
        public decimal OverallocatedCost { get; set; }
        public decimal WasteReduction => TotalCost == 0 ? 0 : (WasteCost / TotalCost) * 100;
        public decimal OverallocatedReduction => TotalCost == 0 ? 0 : (OverallocatedCost / TotalCost) * 100;
    }

    public class CountSampleDispersionReportPost : ReportRequest
    {
        public string CountType { get; set; }
        public List<int> SpecialtyId { get; set; }
        public List<int> SurgeonId { get; set; }
        public List<int> TrayId { get; set; }
        public List<int> ItemId { get; set; }
        public List<int> CardCategoryId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Group { get; set; }
    }
    public class CountSampleDispersionReport
    {
        public string GroupValue { get; set; }
        public string Surgeon { get; set; }
        public string Card { get; set; }
        public int CardId { get; set; }
        public string Tray { get; set; }
        public string Item { get; set; }
        public int InstrumentCount { get; set; }
        public int ConsolidationCount { get; set; }
        public int CardCount { get; set; }
    }
    public class CountSampleVelocity
    {
        public int CardId { get; set; }
        public int Jan { get; set; }
        public int Feb { get; set; }
        public int Mar { get; set; }
        public int Apr { get; set; }
        public int May { get; set; }
        public int Jun { get; set; }
        public int Jul { get; set; }
        public int Aug { get; set; }
        public int Sep { get; set; }
        public int Oct { get; set; }
        public int Nov { get; set; }
        public int Dec { get; set; }
        public int JanYear => DateTime.Today.Year - (DateTime.Today.Month >= 1 ? 0 : 1);
        public int FebYear => DateTime.Today.Year - (DateTime.Today.Month >= 2 ? 0 : 1);
        public int MarYear => DateTime.Today.Year - (DateTime.Today.Month >= 3 ? 0 : 1);
        public int AprYear => DateTime.Today.Year - (DateTime.Today.Month >= 4 ? 0 : 1);
        public int MayYear => DateTime.Today.Year - (DateTime.Today.Month >= 5 ? 0 : 1);
        public int JunYear => DateTime.Today.Year - (DateTime.Today.Month >= 6 ? 0 : 1);
        public int JulYear => DateTime.Today.Year - (DateTime.Today.Month >= 7 ? 0 : 1);
        public int AugYear => DateTime.Today.Year - (DateTime.Today.Month >= 8 ? 0 : 1);
        public int SepYear => DateTime.Today.Year - (DateTime.Today.Month >= 9 ? 0 : 1);
        public int OctYear => DateTime.Today.Year - (DateTime.Today.Month >= 10 ? 0 : 1);
        public int NovYear => DateTime.Today.Year - (DateTime.Today.Month >= 11 ? 0 : 1);
        public int DecYear => DateTime.Today.Year - (DateTime.Today.Month >= 12 ? 0 : 1);
    }
    public class CountSampleDispersionReportCard
    {
        public string Card { get; set; }
        public string Surgeon { get; set; }
        public string GroupValue { get; set; }
        public int CardCount { get; set; }
        public CountSampleVelocity Velocity { get; set; }
        public List<CountSampleDispersionReport> Instruments { get; set; }
    }
    public class CountSampleDispersionReportSurgeon
    {
        public string Surgeon { get; set; }
        public string GroupValue { get; set; }
        public List<CountSampleDispersionReportCard> Cards { get; set; }
    }
    public class CountSummaryReportPost : ReportRequest
    {
        public List<int> SpecialtyId { get; set; }
        public List<int> SurgeonId { get; set; }
        public List<int> CardId { get; set; }
        public List<int> CardCategoryId { get; set; }
        public List<int> RoomGroupId { get; set; }
        public List<int> ItemCategoryId { get; set; }
        public List<int> ItemId { get; set; }
        public string Order { get; set; }
        public string Group { get; set; }
    }

    public class SupplyDistributionPost : CountSummaryReportPost
    {
        public int? MinCost { get; set; }
        public string CardFilter { get; set; }
        public string SurgeonFilter { get; set; }
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
        public string CardCategory { get; set; }
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
