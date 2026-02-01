using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Administration
{
    public class TrayImport : IImportData
    {
        public string TrayType { get; set; }
        public string InstrumentType { get; set; }
        public string TrayID { get; set; }
        public string TrayName { get; set; }
        public string InstrumentName { get; set; }
        public int Quantity { get; set; }
        public string Manufacturer { get; set; }
        public string Category { get; set; }
        public bool VendorTray { get; set; }
        public string InstrumentNumber { get; set; }
        public int Sequence { get; set; }
    }
}
