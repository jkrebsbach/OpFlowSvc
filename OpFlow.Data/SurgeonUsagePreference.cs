using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{

    public class SurgeonUsagePreferenceSummary
    {
        public List<SurgeonUsagePreference> VendorTrays { get; set; }
        public List<SurgeonUsagePreference> InternalTrays { get; set; }
        public List<SurgeonUsagePreference> Supplies { get; set; }
    }

    public class SurgeonUsagePreference
    {
        public string TrayName { get; set; }
        public string ItemName { get; set; }
        public string VendorID { get; set; }
        public bool VendorTray => VendorID != null;


        public int PersonalCases { get; set; }
        public int PeerCases { get; set; }
        public int BaselineCases { get; set; }

        public int PersonalNet { get; set; }
        public int PeerNet { get; set; }
        public int BaselineNet { get; set; }

        public decimal? AvgUsed => PersonalCases == 0 ? 0 : (PersonalNet / PersonalCases);
        public decimal? PeerUsed => PeerCases == 0 ? 0 : (PeerNet / PeerCases);
        public decimal? BaselineUsed => BaselineCases == 0 ? 0 : (BaselineNet / BaselineCases);
    }
}
