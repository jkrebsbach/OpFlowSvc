using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpFlow.Data
{
    public class SurgeonUsagePreferenceSummary
    {
        public List<SurgeonUsageTraySummary> VendorTrays { get; set; }
        public List<SurgeonUsageTraySummary> InternalTrays { get; set; }
        public List<SurgeonUsagePreference> Supplies { get; set; }
    }

    public class SurgeonUsageTraySummary
    {
        public string TrayName { get; set; }

        public decimal PersonalOpen => Instruments.Max(i => i.PersonalOpen);
        public decimal PeerOpen => Instruments.Max(i => i.PeerOpen);
        public decimal BaselineOpen => Instruments.Max(i => i.BaselineOpen);

        public decimal PersonalCases => Instruments.Max(i => i.PersonalCases);
        public decimal PeerCases => Instruments.Max(i => i.PeerCases);
        public decimal BaselineCases => Instruments.Max(i => i.BaselineCases);

        public List<SurgeonUsagePreference> Instruments { get; set; }
    }

    public class SurgeonUsagePreference
    {
        public string TrayName { get; set; }
        public string ItemName { get; set; }
        public string VendorID { get; set; }
        public int ItemQty { get; set; }
        public bool VendorTray => VendorID != null;

        public int? TrayOpenCount { get; set; }

        public decimal PersonalOpen { get; set; }
        public decimal PeerOpen { get; set; }
        public decimal BaselineOpen { get; set; }

        public decimal PersonalCases { get; set; }
        public decimal PeerCases { get; set; }
        public decimal BaselineCases { get; set; }

        public decimal PersonalNet { get; set; }
        public decimal PeerNet { get; set; }
        public decimal BaselineNet { get; set; }

        public decimal? AvgUsage => PersonalOpen == 0 ? 0 : (PersonalNet / PersonalOpen);
        public decimal? PeerUsage => PeerOpen == 0 ? 0 : (PeerNet / PeerOpen);
        public decimal? BaselineUsage => BaselineOpen == 0 ? 0 : (BaselineNet / BaselineOpen);
    }
}
