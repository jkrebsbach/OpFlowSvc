using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class ItemRationalization : ItemMaster
    {
        public List<ItemCard> Cards { get; set; }

        public ItemRationalization()
        {
            Cards = new List<ItemCard>();
        }
    }

    public class ItemCard
    {
        public int CardID { get; set; }
        public int ItemID { get; set; }
        public string CardName { get; set; }
        public string Surgeon { get; set; }
        public int QtyOpen { get; set; }
        public string QtyHold { get; set; }
        public decimal? AvgSetup { get; set; }
        public decimal? AvgUsed { get; set; }
        public decimal? WasteAvg { get; set; }
    }

    public class ItemRationalizationCase
    {
        public int SurgeryID { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan ScheduleTime { get; set; }
        public string RoomDescription { get; set; }
        public string CardDescription { get; set; }
        public string SurgeonName { get; set; }
        public int? DisposableAuditID { get; set; }
        public int? DisposableCountID { get; set; }

        public List<ItemRationalizationCaseItem> Items { get; set; }
        
        public DateTime ScheduleDateTime => ScheduleDate.Add(ScheduleTime);

        public ItemRationalizationCase()
        {
            Items = new List<ItemRationalizationCaseItem>();
        }
    }

    public class ItemRationalizationCaseItem
    {
        public int SurgeryID { get; set; }
        public int ItemID { get; set; }
        public string ItemDescription { get; set; }
    }

    public class ItemAuditPost
    {
        public List<int> Surgeries { get; set; }
        public string Target { get; set; }
    }

    public class ItemAuditPut
    {
        public List<ItemAuditUpdate> Audits { get; set; }
        public string Target { get; set; }
    }

    public class ItemAuditUpdate
    {
        public int SurgeryID { get; set; }
        public string Comment { get; set; }
    }
}
