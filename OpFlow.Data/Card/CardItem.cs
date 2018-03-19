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
        public int QtyHold { get; set; }
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

    public class CardItemCountResult
    {
        public List<CardItemCount> Supplies { get; set; }
        public List<CardItemCount> Instruments { get; set; }
        public Dictionary<int, List<CardItemCount>> Trays { get; set; }

        public CardItemCountResult()
        {
            Trays = new Dictionary<int, List<CardItemCount>>();
        }
    }
}
