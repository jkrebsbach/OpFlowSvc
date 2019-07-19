using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class ItemRationalization : ItemMaster
    {
        public List<ItemCard> Cards { get; set; }

        public ItemRationalization()
        {
            Cards = new List<ItemCard>();
        }
    }

    public class ItemCard
    {
        public int CardID { get; set; }
        public int ItemID { get; set; }
        public string CardName { get; set; }
        public string Surgeon { get; set; }
        public int QtyOpen { get; set; }
        public string QtyHold { get; set; }
        public decimal? AvgUsed { get; set; }
        public decimal? WasteAvg { get; set; }
    }
}
