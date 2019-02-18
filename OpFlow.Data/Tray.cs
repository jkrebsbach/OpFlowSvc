using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{

    public class TrayUsage
    {
        public int TrayID { get; set; }
        public bool TrayOpened { get; set; }
        public List<CardItemCount> TrayItems { get; set; }
    }
    public class TrayQuestion
    {
        public int CollectionItemID { get; set; }
        public int QuestionID { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }
    }
}
