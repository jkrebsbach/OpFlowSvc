using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Administration
{
    public class ItemImport : IImportData
    {
        public string Catalog { get; set; }
        public string EHR_ID { get; set; }
        public string ItemType { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string UnitOfMeasure { get; set; }
        public string Manufacturer { get; set; }
        public string Cost { get; set; }

    }
}
