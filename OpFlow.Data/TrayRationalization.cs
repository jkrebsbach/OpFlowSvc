using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{

    public class TrayRationalizationHeader
    {
        public List<Specialty> Specialties { get; set; }
        public List<TrayRationalization> Proposals { get; set; }
        public List<GetProposedTrayInstrumentCategories> Categories { get; set; }
        public List<ItemMaster> Trays { get; set; }
        public List<Surgeon> Surgeons { get; set; }
        public List<Card> Cards { get; set; }
        public List<Vendor> Vendors { get; set; }
        public List<TrayProposalPhase> Phases { get; set; }
        public List<TrayRationalization> StandardizedTrays { get; set; }
        public List<TrayQuestionSummary> Questions { get; set; }
        public bool Vendor { get; set; }
    }

    public class TrayRationalization
    {
        public int TrayProposalID { get; set; }
        public int? VendorID { get; set; }
        public int? TrayProposalPhaseID { get; set; }
        public int? SpecialtyID { get; set; }
        public string Vendor { get; set; }
        public string Status { get; set; }
        public string StatusName { get; set; }
        public string TrayName { get; set; }
        public string TrayProposalPhase { get; set; }
        public string Specialty { get; set; }
    }

    public class SourceTraySummary
    {
        public string TrayName { get; set; }
        public int CountsComplete { get; set; }
        public int CountsScheduled { get; set; }
        public int AuditsComplete { get; set; }
        public int AuditsScheduled { get; set; }
    }

    public class TrayProposalPhase
    {
        public int TrayProposalPhaseID { get; set; }
        public string TrayProposalPhaseDescription { get; set; }
    }

    public class TrayRationalizationItem : ItemTrayOverlap
    {
        public int TrayProposalID { get; set; }
        public int? CurrentInstrumentID { get; set; }
        public int InstrumentCount { get; set; }
        public decimal AvgPerCase { get; set; }
        public int SourceQty { get; set; }
        public int TrayProposalCategoryID { get; set; }
        public string SubCategory { get; set; }
        public string Description { get; set; }
        public string Range { get; set; }
        public int TrayCases { get; set; }
        public int UsedCases { get; set; }
        public string Reason { get; set; }
        public string Comments { get; set; }
        public string HistoryType { get; set; }
        public int Proposed { get; set; }
        public DateTimeOffset? UpdTimestamp { get; set; }

        public decimal CaseUsagePcnt => TrayCases == 0 ? 0 : (UsedCases / (decimal)TrayCases * 100);
    }

    public class TrayRationalizationStatusLog
    {
        public string Status { get; set; }
        public string Phase { get; set; }
        public string StatusName { get; set; }
        public string StatusUser { get; set; }
        public DateTimeOffset StatusDate { get; set; }

    }

    public class TrayRationalizationCard
    {
        public string SurgeonName { get; set; }
        public string CardName { get; set; }
        public string SpecialtyName { get; set; }
        public string SourceTrayName { get; set; }
        public string NewTrayName { get; set; }
        public string InstrumentName { get; set; }
        public int Proposed { get; set; }
    }

    public class TrayRationalizationConfigPost
    {
        public List<int> Specialties { get; set; }
        public List<int> Trays { get; set; }
        public List<int> Surgeons { get; set; }
        public List<int> Cards { get; set; }
        public List<TrayQuestion> Questions { get; set; }
        public string CptCode { get; set; }
    }

    public class CardListPost
    {
        public List<CardListTrayPost> Trays { get; set; }
    }

    public class CardListTrayPost
    {
        public int CardID { get; set; }
        public int TrayID { get; set; }
    }

    public class ProposedTrayPost
    {
        public int? VendorID { get; set; }
        public int? PhaseID { get; set; }
        public int? SpecialtyID { get; set; }
        public string TrayName { get; set; }
        public string Status { get; set; }
        public List<ProposedTrayInstrumentPost> Instruments { get; set; }
    }

    public class ProposedTrayInstrumentPost
    {
        public int InstrumentID { get; set; }
        public int TrayItemID { get; set; }
        public int Quantity { get; set; }
    }

    public class ProposedTrayUpdatePost
    {
        public List<UpdateTrayInstrumentPost> Instruments { get; set; }
    }

    public class UpdateTrayInstrumentPost
    {
        public int InstrumentID { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public int? CategoryID { get; set; }
        public string SubCategory { get; set; }
        public string Description { get; set; }
        public string Range { get; set; }
        public string Comments { get; set; }
        public int Sequence { get; set; }
    }

    public class AuditDetailPost
    {
        public List<UpdateAuditDetail> Audits { get; set; }
    }

    public class UpdateAuditDetail
    {
        public int SurgeryID { get; set; }
        public List<string> SurgeryCpts { get; set; }
        public string Comments { get; set; }
    }

    public class TrayRationalizationComparePost
    {
        public int? TrayID { get; set; }
        public decimal Overlap { get; set; }
        public decimal Buffer { get; set; }
    }

    public class TrayRationalizationCompare
    {
        public string ProposedTrayName { get; set; }
        public string ExistingTrayName { get; set; }
        public string InstrumentName { get; set; }
        public int UsedInstruments { get; set; }
        public int CurrentCards { get; set; }
        public int SatisfiedCards { get; set; }
        public int BufferedCards { get; set; }
        public int CommonInstruments { get; set; }
        public decimal OverlapPcnt =>
            (CommonInstruments == 0 ? 0 : (decimal)UsedInstruments / CommonInstruments * 100);
    }

    public class TrayRationalizationDetailPost
    {
        public int CardID { get; set; }
        public int TrayID { get; set; }
    }

    public class TrayRationalizationOverlapPost
    {
        public List<string> Trays { get; set; }
        public int? StandardizedTrayID { get; set; }
    }

    public class TrayRationalizationDetail
    {
        public string SurgeonName { get; set; }
        public string CardName { get; set; }
        public string TrayName { get; set; }
        public string InstrumentName { get; set; }
        public int QtyOpen { get; set; }
        public int AvgUsed { get; set; }
        public int ProposedQty { get; set; }
        public int PeelPackQty { get; set; }
        public string PeelPackStatus { get; set; }
    }

    public class TrayCardOverlapSummary
    {
        public string TrayName { get; set; }
        public int NbrInstances { get; set; }
        public int NbrInstruments { get; set; }
        public decimal CostPerTray { get; set; }
        public decimal CostAllTrays { get; set; }
        public int ProcessedAvg { get; set; }
        public int ProcessedMin { get; set; }
        public int ProcessedMax { get; set; }
        public decimal UsedInstruments { get; set; }
        public int CommonInstruments { get; set; }
        public decimal? OverlapPcnt =>
            (CommonInstruments == 0 ? 0 : (decimal)UsedInstruments / CommonInstruments * 100);
    }

    public class GetProposedTrayInstrumentCategories
    {
        public int CategoryID { get; set; }
        public string CategoryDescription { get; set; }
    }

    public class TrayCardOverlap
    {
        public int CardID { get; set; }
        public int TrayID { get; set; }
        public string SpecialtyName { get; set; }
        public string CardDescription { get; set; }
        public string SurgeonName { get; set; }
        public int TimesUsed { get; set; }
        public string TrayName { get; set; }
        public int CoveredInstruments { get; set; }
        public int CurrentTrayItems { get; set; }
        public int UsedInstruments { get; set; }
        public int CommonInstruments { get; set; }
        public decimal Overlap => (decimal)CoveredInstruments / CurrentTrayItems * 100;
        public decimal? OverlapPcnt =>
            (CommonInstruments == 0 ? 0 : (decimal)UsedInstruments / CommonInstruments * 100);
    }

    public class TraySurgeryAudit
    {
        public int TrayProposalID { get; set; }
        public int SurgeryID { get; set; }
        public int SpecialtyID { get; set; }
        public int? AuditUserID { get; set; }
        public string ProposedTrayName { get; set; }
        public string SurgeonName { get; set; }
        public string RoomDescription { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public string ScrubTechUser { get; set; }
        public string AuditUser { get; set; }
        public string TrayStatus { get; set; }
        public string AuditComments { get; set; }
        public string AuditType { get; set; }

        public string CptCode1 => CptCodes.Count > 0 ? CptCodes[0]?.CptCode : null;
        public string CptCode2 => CptCodes.Count > 1 ? CptCodes[1]?.CptCode : null;
        public string CptCode3 => CptCodes.Count > 2 ? CptCodes[2]?.CptCode : null;

        public List<SurgeryUser> ScrubTechs { get; set; }
        public List<SurgeryAuditSourceTray> SourceTrays { get; set; }
        public List<SurgeryCPTCode> CptCodes { get; set; }

        public TraySurgeryAudit()
        {
            ScrubTechs = new List<SurgeryUser>();
            SourceTrays = new List<SurgeryAuditSourceTray>();
            CptCodes = new List<SurgeryCPTCode>();
        }
    }

    public class TrayRationalizationExport : TrayRationalizationItem
    {
        public int ProposedQuantity { get; set; }
        public int SourceQuantity { get; set; }
    }

    public class ProposedTrayExport
    {
        public List<TrayRationalizationExport> ProposedInstruments { get; set; }
        public List<TrayRationalizationExport> SourceInstruments { get; set; }
    }

    public class SurgeryAuditSourceTray
    {
        public int TrayID { get; set; }
        public int TrayProposalID { get; set; }
        public int SurgeryID { get; set; }
        public string TrayName { get; set; }
    }

    public class SurgeryAuditSearchResult : SurgerySearchResult
    {
        public string SurgeonName { get; set; }
        public int? AuditID { get; set; }

        public List<SurgeryAuditTray> Trays { get; set; }

        public SurgeryAuditSearchResult()
        {
            Trays = new List<SurgeryAuditTray>();
        }
    }

    public class SurgeryAuditTray
    {
        public int SurgeryID { get; set; }
        public string TrayName { get; set; }
    }

    public class AddCaseAuditPost
    {
        public List<int> Surgeries { get; set; }
        public string Target { get; set; }
    }
}
