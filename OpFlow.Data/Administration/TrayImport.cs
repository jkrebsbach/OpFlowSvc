using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Administration
{
    public class TrayImport : IImportData
    {
        public string Customer { get; set; }
        public string TrayID { get; set; }
        public string TrayName { get; set; }
        public string InstrumentName { get; set; }
        public int Quantity { get; set; }
        public string Manufacturer { get; set; }
    }
}
