using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class CardItemFeedback
    {
        public int CardFeedbackID { get; set; }
        public string SurgeonName { get; set; }
        public string SpecialtyName { get; set; }
        public string CardName { get; set; }
        public int ItemID { get; set; }
        public string CatalogID { get; set; }
        public int EHR_ID { get; set; }
        public bool CountNeeded { get; set; }
        public string UnitOfMeasure { get; set; }
        public string ItemType { get; set; }
        public string ItemDescription { get; set; }
        public decimal UnitCost { get; set; }
        public decimal BillableUnitCost { get; set; }
        public string Manufacturer { get; set; }
        public int VendorID { get; set; }
        public int CardID { get; set; }
        public int QtyOpen { get; set; }
        public decimal QtyOpenCost { get; set; }
        public string QtyHold { get; set; }
        public int? QtyOpenOrig { get; set; }
        public string QtyHoldOrig { get; set; }
        public string QtySource { get; set; }
        public DateTimeOffset? RequestDate { get; set; }
        public bool DeleteItem { get; set; }
    }

    public class FeedbackRequest
    {
        public bool Response { get; set; }
    }
}
