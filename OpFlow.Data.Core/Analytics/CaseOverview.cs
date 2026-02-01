using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Analytics
{
    public class CaseOverview
    {
        public int CaseCount { get; set; }
        public int ClosedCount { get; set; }
    }
    public class CaseSurgeonOverview : CaseOverview
    {
        public string SurgeonName { get; set; }
    }
    public class CaseBundleOverview : CaseOverview
    {
        public string BundleName { get; set; }
    }

    public class OverviewScreen
    {
        public CaseOverview CaseOverview { get; set; }
        public List<CaseSurgeonOverview> CaseSurgeonOverview { get; set; }
        public List<CaseBundleOverview> CaseBundleOverview { get; set; }
    }
}
