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
        public List<ItemMaster> Trays { get; set; }
        public List<User> Surgeons { get; set; }
        public List<Card> Cards { get; set; }
    }

    public class TrayRationalization
    {
        public int TrayItemID { get; set; }
        public string TrayName { get; set; }
        public int CaseCount { get; set; }
        public int InstrumentCount { get; set; }
        public int AvgUsage { get; set; }
    }
}
