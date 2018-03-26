using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Administration
{
    public class CardImport : IImportData
    {
        public string Location { get; set; }
        public string Surgeon { get; set; }
        public string PreferenceCardName { get; set; }
        public string Type { get; set; }
        public string LawsonID { get; set; }
        public string CatalogNbr { get; set; }
        public string SupplyDescription { get; set; }
        public string Manufacturer { get; set; }
        public string OpenAmt { get; set; }

        public string PrnRequired { get; set; }
        public string CostPerUnitOt { get; set; }
        public string Dosage { get; set; }
        public string Unit { get; set; }

    }
}
