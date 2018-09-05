using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class CardItem : ItemMaster
    {
        public int CardID { get; set; }
        public int QtyOpen { get; set; }
        public decimal QtyOpenCost { get; set; }
        public string QtyHold { get; set; }
    }

    public class CardItemCount : ItemMaster
    {
        public int CardID { get; set; }
        public int? TrayID { get; set; }
        public int Quantity { get; set; }


        public bool CustomUsage { get; set; }
        public bool Pass1 { get; set; }
        public bool Pass2 { get; set; }
        public bool Pass3 { get; set; }
        public int Usage { get; set; }
    }

    public class SurgeryTrayOpen
    {
        public int SurgeryID { get; set; }
        public int TrayID { get; set; }
        public bool TrayOpened { get; set; }
    }

    public class CardItemCountResult
    {
        public List<CardItemCount> Supplies { get; set; }
        public List<CardItemCount> Instruments { get; set; }
        public List<TrayUsage> Trays { get; set; }

        public CardItemCountResult()
        {
            Trays = new List<TrayUsage>();
        }
    }

    public class TrayUsage
    {
        public int TrayID { get; set; }
        public bool TrayOpened { get; set; }
        public List<CardItemCount> TrayItems { get; set; }
    }
}
