using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class OpFlowProvider
    {
        public int ProviderID { get; set; }
        public string ProviderName { get; set; }
        public string RoleName { get; set; }

        public List<OpFlowLocation> Locations { get; set; }

        public OpFlowProvider()
        {
            Locations = new List<OpFlowLocation>();
        }
    }

    public class OpFlowLocation
    {
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public string LocationName { get; set; }
    }

    public class LocationPost
    {
        public string Provider { get; set; }
        public string Location { get; set; }
    }
}
