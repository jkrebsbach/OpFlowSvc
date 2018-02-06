using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class PatientCase
    {
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public int PatientID { get; set; }
        public int UserID { get; set; }
        public int SpecialtyID { get; set; }

    }
}
