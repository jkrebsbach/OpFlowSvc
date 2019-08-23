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

    public class TrayPlanInstrument
    {
        public int InstrumentID { get; set; }
        public int TrayID { get; set; }
        public string ItemType { get; set; }
        public string InstrumentDescription { get; set; }
        public string TrayName { get; set; }
        public int ProposedQty { get; set; }
    }
}
