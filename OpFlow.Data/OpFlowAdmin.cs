using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class OpFlowSetup
    {
        public List<OpFlowProvider> Providers { get; set; }
        public List<OpFlowLocation> Locations { get; set; }
    }

    public class OpFlowProvider
    {
        public int ProviderID { get; set; }
        public string ProviderName { get; set; }
    }

    public class OpFlowLocation
    {
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public string LocationName { get; set; }
    }
}
