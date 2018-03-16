using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class ItemMaster
    {
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
    }

    public class ItemTray
    {
        public int TrayID { get; set; }
        public string TrayName { get; set; }
    }
}
