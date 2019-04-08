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
        public List<ItemMaster> Proposals { get; set; }
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
        public int InstrumentCount { get; set; }
        public decimal InstrumentCost { get; set; }
        public int Quantity { get; set; }
        public int AvgUsed { get; set; }
        public bool Warning { get; set; }
    }

    public class TrayRationalizationCard
    {
        public string SurgeonName { get; set; }
        public string CardName { get; set; }
        public string SpecialtyName { get; set; }
        public string SourceTrayName { get; set; }
        public string NewTrayName { get; set; }
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
        public List<ProposedTrayInstrumentPost> Instruments { get; set; }
    }

    public class ProposedTrayInstrumentPost
    {
        public int InstrumentID { get; set; }
        public int TrayItemID { get; set; }
        public int Quantity { get; set; }
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
        public int CommonInstruments { get; set; }
        public int CurrentCards { get; set; }
        public int SatisfiedCards { get; set; }
        public int BufferedCards { get; set; }
        public int CurrentTrayItems { get; set; }
        public decimal OverlapPcnt => (decimal)CommonInstruments / CurrentTrayItems * 100;
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
        public int CommonInstruments { get; set; }
        public decimal? OverlapPcnt => (decimal)CommonInstruments / NbrInstruments * 100;
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
        public decimal Overlap => (decimal)CoveredInstruments / CurrentTrayItems * 100;
    }
}
