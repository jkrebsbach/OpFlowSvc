using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Debrief
{
    public class SmartPhrase
    {
        public int SmartPhraseID { get; set; }
        public int SpecialtyID { get; set; }
        public string Category { get; set; }
        public string Phrase { get; set; }
        public string SmartPhraseStep { get; set; }
    }
}
