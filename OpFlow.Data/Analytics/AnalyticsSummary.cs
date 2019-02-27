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
        public string InstrumentName { get; set; }
        public string TrayName { get; set; }
        public int InstrumentCount { get; set; }
        public int QtyOpen { get; set; }
        public string QtyHold { get; set; }
        public int AvgUsed { get; set; }
    }

    public class TrayRationalizationConfigPost
    {
        public List<int> Specialties { get; set; }
        public List<int> Trays { get; set; }
        public List<int> Surgeons { get; set; }
        public List<int> Cards { get; set; }
    }
}
