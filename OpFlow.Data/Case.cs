using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Case
    {
        public int CaseID { get; set; }
        public int ProviderID { get; set; }
        public int PatientID { get; set; }
        public int CardID { get; set; }
        public string ProviderLastName { get; set; }
        public string ProviderFirstName { get; set; }

        public Card Card { get; set; }
    }
}
