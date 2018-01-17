using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class Card
    {
        public int CardID { get; set; }
        public int ProcedureID { get; set; }
        public int SpecialtyID { get; set; }
        public string ProcedureDescription { get; set; }
        public string CardDescription { get; set; }
    }
}
