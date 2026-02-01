using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Debrief
{
    public class SmartPhrase
    {
        public int SmartPhraseID { get; set; }
        public int SpecialtyID { get; set; }
        public int CategoryID { get; set; }
        public int UserID { get; set; }
        public int DefaultStepID { get; set; }
        public int DefaultRoleID { get; set; }
        public string Phrase { get; set; }
        public string RoleName { get; set; }
        public string CategoryName { get; set; }
        public string SpecialtyName { get; set; }
        public string UserName { get; set; }
    }
}
