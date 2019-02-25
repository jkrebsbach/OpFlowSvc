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
    public class TrayQuestionSummary
    {
        public int QuestionID { get; set; }
        public string Question { get; set; }

        public List<TrayQuestion> Answers { get; set; }
    }

    public class TrayHistory 
    {
        public string SpecialtyDescription { get; set; }
        public string CollectionDescription { get; set; }
        public string TrayName { get; set; }
        public string SurgeonName { get; set; }
        public string InstrumentDescription { get; set; }
        public decimal AvgUsed { get; set; }
    }

    public class TrayHistoryRequest
    {
        public List<TrayQuestion> Questions { get; set; }
    }

    public class TrayInstrumentPost
    {
        public string InstrumentName { get; set; }
        public string InstrumentNbr { get; set; }
        public int Quantity { get; set; }
    }
}
