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

    public class OpFlowLocation : Location
    {
        public int TrayCount { get; set; }
        public int CardCount { get; set; }

        public int? TrayHigh { get; set; }
        public int? TrayMed { get; set; }
        public int? TrayLow { get; set; }
        public int? SurgeonHigh { get; set; }
        public int? SurgeonMed { get; set; }
        public int? SurgeonLow { get; set; }
        public int? AuditHigh { get; set; }
        public int? AuditMed { get; set; }
        public int? AuditLow { get; set; }
        public int? DailyTarget { get; set; }
        public bool ActiveLocation { get; set; }
    }

    public class LocationPost
    {
        public string Provider { get; set; }
        public string Location { get; set; }
    }

    public class UserSettingsPost
    {
        public string UserSettings { get; set; }
    }
}
