using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Administration
{
    public class ProposedTrayImport : IImportData
    {
        public string ProposedTrayName { get; set; }
        public string TrayName { get; set; }
        public string InstrumentName { get; set; }
        public string InstrumentType { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }
    }
}
