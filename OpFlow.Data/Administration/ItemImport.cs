using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Administration
{
    public class ItemImport : IImportData
    {
        public string Location { get; set; }
        public string LocationName { get; set; }
        public string FromLoc { get; set; }
        public string BinSeq { get; set; }
        public string Bin { get; set; }
        public string ItemNbr { get; set; }
        public string Desc { get; set; }
        public string ManuName { get; set; }
        public string MfgNbr { get; set; }

        public string ParLevel { get; set; }
        public string UOM { get; set; }
        public string ItemCost { get; set; }
        public string InventoryValue { get; set; }

    }
}
