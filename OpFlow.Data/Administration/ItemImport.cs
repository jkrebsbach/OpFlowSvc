using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Administration
{
    public class ItemImport : IImportData
    {
        public string ItemID { get; set; }
        public string Catalog { get; set; }
        public string EMRID { get; set; }
        public string Type { get; set; }
        public int Category { get; set; }

        public string Description { get; set; }

        public string UnitOfMeasure { get; set; }
        public string Manufacturer { get; set; }
        public string Cost { get; set; }

    }
}
