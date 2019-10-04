using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpFlow.Data.Analytics
{
    public class TrayConsolidation
    {
        public int TrayItemID { get; set; }
        public string TrayName { get; set; }
        public int TrayInstances { get; set; }
        public int? CardCount { get; set; }
        public int TrayInstrumentCount { get; set; }
        public string Specialty { get; set; }
        public int? SpecialtyID { get; set; }
        public string CardCategory { get; set; }
        public int TrayAudits { get; set; }
        public int TrayCounts { get; set; }
        public decimal TrayAvgUsage { get; set; }
        public int? SecondTrayItemID { get; set; }
        public string SecondTray { get; set; }
        public int SecondTrayInstances { get; set; }
        public int RedundantInstruments { get; set; }
        public int SecondCardCount { get; set; }
        public int SecondInstrumentCount { get; set; }
        public int OverlapCardCount { get; set; }
        public decimal OverlapPcnt { get; set; }
        public int? SecondaryAudits { get; set; }
        public int? SecondaryCounts { get; set; }
        public decimal? SecondaryAvgUsage { get; set; }
        public int SpecialtyCardCount { get; set; }
        public int SpecialtyTrayCount { get; set; }
    }

    public class TrayConsolidationResult
    {
        public int? SpecialtyID { get; set; }
        public string SpecialtyName { get; set; }
        public int SpecialtyTrayCount { get; set; }
        public int SpecialtyCardCount { get; set; }
        public decimal AvgTrayCount => SpecialtyCardCount == 0 ? 0 : (decimal)SpecialtyTrayCount / SpecialtyCardCount;
        public List<TrayConsolidationTray> Consolidations { get; set; }

        public static List<TrayConsolidationResult> Summarize(List<TrayConsolidation> source)
        {
            var result = new List<TrayConsolidationResult>();

            foreach (var specialty in source.GroupBy(s => s.SpecialtyID))
            {
                var parent = new TrayConsolidationResult()
                {
                    SpecialtyID = specialty.Key,
                    SpecialtyName = specialty.First().Specialty,
                    SpecialtyTrayCount = specialty.First().SpecialtyTrayCount,
                    SpecialtyCardCount = specialty.First().SpecialtyCardCount,
                    Consolidations = new List<TrayConsolidationTray>()
                };
                result.Add(parent);

                foreach (var tray in specialty.OrderByDescending(t => t.CardCount).GroupBy(t => t.TrayItemID))
                {
                    var entity = tray.First();

                    var trayConsolidation = new TrayConsolidationTray()
                    {
                        TrayItemID = entity.TrayItemID,
                        TrayName = entity.TrayName,
                        CardCount = entity.CardCount,
                        TrayInstrumentCount = entity.TrayInstrumentCount,
                        TrayInstances = entity.TrayInstances,
                        TrayAudits = entity.TrayAudits,
                        TrayCounts = entity.TrayCounts,
                        TrayAvgUsage = entity.TrayAvgUsage,
                        CardCategory = entity.CardCategory,
                        Children = new List<TrayConsolidationTray>()
                    };
                    parent.Consolidations.Add(trayConsolidation);

                    foreach (var child in tray.OrderByDescending(t => t.SecondCardCount))
                    {
                        if (child.SecondTrayItemID == null)
                            continue;

                        var secondary = new TrayConsolidationTray()
                        {
                            TrayItemID = child.SecondTrayItemID.Value,
                            TrayName = child.SecondTray,
                            CardCount = child.SecondCardCount,
                            RedundantInstruments = child.RedundantInstruments,
                            OverlapCardCount = child.OverlapCardCount,
                            OverlapPcnt = child.OverlapPcnt * 100,
                            TrayInstrumentCount = child.SecondInstrumentCount,
                            TrayInstances = child.SecondTrayInstances,
                            TrayAudits = child.SecondaryAudits ?? 0,
                            TrayCounts = child.SecondaryCounts ?? 0,
                            TrayAvgUsage = child.SecondaryAvgUsage ?? 0,
                            CardCategory = child.CardCategory
                        };

                        trayConsolidation.Children.Add(secondary);
                    }
                }
            }

            return result;
        }

        public static List<TrayConsolidationTray> SummarizeChildren(List<TrayConsolidation> source)
        {
            var result = new List<TrayConsolidationTray>();

            foreach (var tray in source)
            {
                var consolidation = new TrayConsolidationTray()
                {
                    TrayName = tray.SecondTray,
                    CardCount = tray.SecondCardCount,
                    RedundantInstruments = tray.RedundantInstruments,
                    OverlapCardCount = tray.OverlapCardCount,
                    OverlapPcnt = tray.OverlapPcnt * 100,
                    TrayInstrumentCount = tray.SecondInstrumentCount,
                    TrayInstances = tray.SecondTrayInstances,
                    TrayAudits = tray.SecondaryAudits ?? 0,
                    TrayCounts = tray.SecondaryCounts ?? 0,
                    TrayAvgUsage = tray.SecondaryAvgUsage ?? 0,
                    CardCategory = tray.CardCategory
                };

                result.Add(consolidation);
            }

            return result;
        }
    }

    public class TrayConsolidationTray
    { 
        public int TrayItemID { get; set; }
        public string TrayName { get; set; }
        public int? CardCount { get; set; }
        public int RedundantInstruments { get; set; }
        public int OverlapCardCount { get; set; }
        public decimal OverlapPcnt { get; set; }
        public int TrayInstrumentCount { get; set; }
        public int TrayInstances { get; set; }
        public int TrayAudits { get; set; }
        public int TrayCounts { get; set; }
        public decimal TrayAvgUsage { get; set; }
        public string CardCategory { get; set; }
        public List<TrayConsolidationTray> Children { get; set; }
    }
}
