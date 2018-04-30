using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Procedure
    {
        public int ProcedureID { get; set; }
        public int SpecialtyID { get; set; }
        public bool ProcedurePerformed { get; set; }
        public string CPTCode { get; set; }
        public string ProcedureStatus { get; set; }
        public string ProcedureDescription { get; set; }
        public string ProcedureSpecialty { get; set; }
        public string SpecialtyDescription { get; set; }
    }

    public class BundleProcedure : Procedure
    {
        public int BundleID { get; set; }
        public string BundleDescription { get; set; }
    }
}
