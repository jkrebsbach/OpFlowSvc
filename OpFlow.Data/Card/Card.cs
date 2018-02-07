using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Card
    {
        public int CardID { get; set; }
        public int LocationID { get; set; }
        public int ProviderID { get; set; }
        public int SurgeryID { get; set; }
        public int ProcedureID { get; set; }
        public int SpecialtyID { get; set; }
        public int FlowID { get; set; }
        public int RoomConfigID { get; set; }
        public int TimesUsed { get; set; }
        public string ProcedureDescription { get; set; }
        public string FlowDescription { get; set; }
        public string CardDescription { get; set; }
        public decimal AvgMinutes { get; set; }
    }

    public class CardQuantityEdit
    {
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public int ItemID { get; set; }
        public int OpenQty { get; set; }
        public int HoldQty { get; set; }
    }
}
