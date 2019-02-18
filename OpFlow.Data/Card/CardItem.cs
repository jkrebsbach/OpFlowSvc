using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class CardItem : ItemMaster
    {
        public int CardID { get; set; }
        public int QtyOpen { get; set; }
        public decimal QtyOpenCost { get; set; }
        public string QtyHold { get; set; }
    }

    public class SurgeryCardItem : CardItem
    {
        public int? QtyOpenOrig { get; set; }
        public string QtyHoldOrig { get; set; }
        public string QtySource { get; set; }

        public bool ValueChanged => QtyOpenOrig != QtyOpen || QtyHoldOrig != QtyHold;
    }

    public class CardItemCountQueryResult
    {
        public List<CardItemCount> CardItemCounts { get; set; }
        public List<CollectionItemCount> TrayCollectionCounts { get; set; }
        public List<TrayQuestion> TrayQuestions { get; set; }
    }

    public class CardItemCount : ItemMaster
    {
        public int CardID { get; set; }
        public int Quantity { get; set; }


        public bool CustomUsage { get; set; }
        public int Usage { get; set; }
        public int? UsageType { get; set; }
    }

    public class CollectionItemCount : CardItemCount
    {
        public int CollectionItemID { get; set; }
    }

    public class TrayCollection : CardItemCount
    {
        public TrayCollection(CardItemCount c)
        {
            CardID = c.CardID;
            TrayID = c.TrayID;
            Quantity = c.Quantity;
            Usage = c.Usage;
            UsageType = c.UsageType;
            ItemDescription = c.ItemDescription;
            ItemType = c.ItemType;
            ItemID = c.ItemID;
        }

        public List<CollectionItemCount> CollectionItems { get; set; }
        public List<TrayQuestion> Questions { get; set; }

    }

    public class SurgeryTrayOpen
    {
        public int SurgeryID { get; set; }
        public int TrayID { get; set; }
        public bool TrayOpened { get; set; }
    }

    public class CardItemCountResult
    {
        public List<CardItemCount> Supplies { get; set; }
        public List<CardItemCount> Instruments { get; set; }
        public List<TrayCollection> Collections { get; set; }
        public List<TrayUsage> Trays { get; set; }

        public CardItemCountResult()
        {
            Trays = new List<TrayUsage>();
        }
    }

    public class CardUsageHistory
    {
        public CardSummary Summary { get; set; }
        public List<ItemUsageHistory> TrayItems { get; set; }
        public List<ItemUsageHistory> Supplies { get; set; }
    }

    public class CardSummary
    {
        public string ProcedureName { get; set; }
        public string ProviderName { get; set; }
        public string CardName { get; set; }
        public int ProcedureCount { get; set; }
        public int CardCount { get; set; }
    }

    public class ItemUsageHistory
    {
        public string TrayName { get; set; }
        public string ItemType { get; set; }
        public string InstrumentDescription { get; set; }
        public int QtyOpen { get; set; }
    }
}
