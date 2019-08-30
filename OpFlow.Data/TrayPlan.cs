using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class TrayPlan
    {
        public int TrayPlanID { get; set; }
        public int? SpecialtyID { get; set; }
        public string PlanName { get; set; }
    }

    public class TrayPlanPost
    {
        public string PlanName { get; set; }
        public int? SpecialtyID { get; set; }
        public List<TrayPlanInstrumentUsage> Instruments { get; set; }
        public List<TrayPlanInstrumentSummary> Details { get; set; }
    }

    public class TrayPlanFilterPost
    {
        public int? TrayPlanID { get; set; }
        public int? SpecialtyID { get; set; }
        public int? InstrumentCategoryID { get; set; }
        public int? TrayID { get; set; }
        public int? InstrumentID { get; set; }
        public int? CardCategoryID { get; set; }
    }

    public class TrayPlanInstrumentUsage
    {
        public int InstrumentID { get; set; }
        public int TrayID { get; set; }
        public bool Main { get; set; }
        public bool Add { get; set; }
        public bool Single { get; set; }
        public bool Peel { get; set; }
    }

    public class TrayPlanInstrumentSummary
    {
        public string Type { get; set; }
        public List<TrayPlanInstrumentDetail> Instruments { get; set; }
    }
    
    public class TrayPlanInstrumentDetail
    {
        public int InstrumentID { get; set; }
        public int TrayID { get; set; }
        public string TargetTray { get; set; }
        public int Quantity { get; set; }
    }

    public class TrayPlanInstrument
    {
        public int InstrumentID { get; set; }
        public int TrayItemID { get; set; }
        public string ItemType { get; set; }
        public string InstrumentName { get; set; }
        public string TrayName { get; set; }
        public string TargetTrayName { get; set; }
        public int ProposedQty { get; set; }
        public int SourceQty { get; set; }
        public decimal AvgUsed { get; set; }
    }

    public class TrayPlanDetail
    {
        public int TrayPlanID { get; set; }
        public int SpecialtyID { get; set; }
        public int AvgInstruments { get; set; }
        public decimal AvgCardTrays { get; set; }
        public decimal AvgRedundantInstrument { get; set; }
        public int ExcessInstruments { get; set; }
    }
}
