using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Administration
{
    public class CardImport : IImportData
    {
        public string Surgeon { get; set; }
        public string PreferenceCardName { get; set; }
        public string ItemName { get; set; }
        public string ProductNbr { get; set; }
        public string ItemType { get; set; }
        public int? Quantity { get; set; }

        public ImportSurgeon PrimarySurgeon
        {
            get
            {
                var result = new ImportSurgeon();

                if (string.IsNullOrEmpty(Surgeon))
                    return result;

                var surgeons = Surgeon.Split('\n');
                var surgeon = surgeons[0];

                return ImportSurgeon.ParseSurgeon(surgeon);
            }
        }
    }
}
