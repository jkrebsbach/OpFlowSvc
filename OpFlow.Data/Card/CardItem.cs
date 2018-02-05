using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class CardItem : ItemMaster
    {
        public int CardID { get; set; }
        public int StepID { get; set; }
        public int QtyOpen { get; set; }
        public decimal QtyOpenCost { get; set; }
        public int QtyHold { get; set; }
    }
}
