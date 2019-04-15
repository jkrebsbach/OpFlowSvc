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
        public string PowerBiLink { get; set; }
        public string InsTimestamp { get; set; }
    }

    public class TrayRationalizationHeader
    {
        public List<Specialty> Specialties { get; set; }
        public List<TrayRationalization> Proposals { get; set; }
        public List<ItemMaster> Trays { get; set; }
        public List<Surgeon> Surgeons { get; set; }
        public List<Card> Cards { get; set; }
    }

    public class TrayRationalization
    {
        public int TrayProposalID { get; set; }
        public int InstrumentID { get; set; }
        public int TrayItemID { get; set; }
        public string InstrumentName { get; set; }
        public string TrayName { get; set; }
        public string Status { get; set; }
        public string StatusName { get; set; }
        public int InstrumentCount { get; set; }
        public decimal InstrumentCost { get; set; }
        public int Quantity { get; set; }
        public decimal AvgUsed { get; set; }
        public string Reason { get; set; }
        public bool Warning { get; set; }
        public string HistoryType { get; set; }
        public int Proposed { get; set; }
        public DateTimeOffset? UpdTimestamp { get; set; }
    }

    public class TrayRationalizationStatusLog
    {
        public string Status { get; set; }
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
        public int UsedInstruments { get; set; }
        public int CommonInstruments { get; set; }
        public decimal? OverlapPcnt => 
            (CommonInstruments == 0 ? 0 : (decimal)UsedInstruments / CommonInstruments * 100);
    }

    public class ItemTrayOverlap : ItemTray
    {
        public decimal InstrumentCost { get; set; }
        public int AvgUsed { get; set; }
        public bool Warning { get; set; }
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
        public int? AuditUserID { get; set; }
        public string ProposedTrayName { get; set; }
        public string SurgeonName { get; set; }
        public string RoomDescription { get; set; }
        public DateTimeOffset ? ScheduleTime { get; set; }
        public string ScrubTechUser { get; set; }
        public string AuditUser { get; set; }
        public string TrayStatus { get; set; }

        public List<SurgeryUser> ScrubTechs { get; set; }
        public List<SurgeryAuditSourceTray> SourceTrays { get; set; }

        public TraySurgeryAudit()
        {
            ScrubTechs = new List<SurgeryUser>();
            SourceTrays = new List<SurgeryAuditSourceTray>();
        }
    }

    public class ProposedTrayExport
    {
        public List<TrayRationalization> ProposedInstruments { get; set; }
        public List<TrayRationalization> SourceInstruments { get; set; }
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

        public List<SurgeryAuditTray> Trays { get; set; }
    }

    public class SurgeryAuditTray
    {
        public int SurgeryID { get; set; }
        public string TrayName { get; set; }
    }
}
