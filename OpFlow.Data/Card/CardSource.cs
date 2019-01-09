using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class CardSource
    {
        public int CardSourceID { get; set; }
        public int CardID { get; set; }
        public int TemplateFlowID { get; set; }
        public string PreferenceCardName { get; set; }
        public string SurgeonName { get; set; }
    }
}
