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
        public string Instrument { get; set; }
        public int? InstrumentID { get; set; }
        public string CardCategory { get; set; }
        public int TrayAudits { get; set; }
        public int TrayCounts { get; set; }
        public int TotalCases { get; set; }
        public int TrayUsageQty { get; set; }
        public int? SecondTrayItemID { get; set; }
        public string SecondTray { get; set; }
        public int SecondTrayInstances { get; set; }
        public int RedundantInstruments { get; set; }
        public int SecondCardCount { get; set; }
        public string SecondCardCategory { get; set; }
        public int SecondInstrumentCount { get; set; }
        public int OverlapCardCount { get; set; }
        public decimal OverlapPcnt { get; set; }
        public int? SecondaryAudits { get; set; }
        public int? SecondaryCounts { get; set; }
        public int? SecondaryCases { get; set; }
        public int? SecondaryUsageQty { get; set; }
        public int SpecialtyCardCount { get; set; }
        public int SpecialtyTrayCount { get; set; }
    }

    public class TrayConsolidationResult
    {
        public string GroupName { get; set; }
        public int? SpecialtyID { get; set; }
        public string SpecialtyName { get; set; }
        public int? InstrumentID { get; set; }
        public string InstrumentName { get; set; }
        public int SpecialtyTrayCount { get; set; }
        public int SpecialtyCardCount { get; set; }
        public decimal AvgTrayCount => SpecialtyCardCount == 0 ? 0 : (decimal)SpecialtyTrayCount / SpecialtyCardCount;
        public List<TrayConsolidationTray> Consolidations { get; set; }

        public static List<TrayConsolidationResult> Summarize(List<TrayConsolidation> source, string group, int? minEffect)
        {
            var result = new List<TrayConsolidationResult>();

            var grouping = group == "S" ? source.GroupBy(s => s.Specialty) :
                group == "SI" ? source.GroupBy(s => s.Specialty + " - " + s.Instrument) : source.GroupBy(s => s.Instrument);

            var identity = 1;

            foreach (var groupData in grouping)
            {
                var groupEntity = groupData.First();
                var parent = new TrayConsolidationResult()
                {
                    GroupName = groupData.Key,
                    SpecialtyID = groupEntity.SpecialtyID,
                    SpecialtyName = groupEntity.Specialty,
                    InstrumentID = groupEntity.SpecialtyID,
                    InstrumentName = groupEntity.Instrument,
                    SpecialtyTrayCount = groupEntity.SpecialtyTrayCount,
                    SpecialtyCardCount = groupEntity.SpecialtyCardCount,
                    Consolidations = new List<TrayConsolidationTray>()
                };

                result.Add(parent);

                foreach (var tray in groupData.OrderByDescending(t => t.CardCount).GroupBy(t => t.TrayItemID))
                {
                    var entity = tray.First();
                    
                    var trayConsolidation = new TrayConsolidationTray()
                    {
                        Identity = identity,
                        TrayItemID = entity.TrayItemID,
                        TrayName = entity.TrayName,
                        CardCount = entity.CardCount,
                        TrayInstrumentCount = entity.TrayInstrumentCount,
                        TrayInstances = entity.TrayInstances,
                        TrayAudits = entity.TrayAudits,
                        TrayCounts = entity.TrayCounts,
                        TotalCases = entity.TotalCases,
                        TrayUsageQty = entity.TrayUsageQty,
                        CardCategory = entity.CardCategory,
                        Children = new List<TrayConsolidationTray>()
                    };
                    parent.Consolidations.Add(trayConsolidation);

                    var instrumentCount = (trayConsolidation.TrayInstances * trayConsolidation.TrayInstrumentCount);

                    foreach (var child in tray.OrderByDescending(t => t.OverlapPcnt))
                    {
                        if (child.SecondTrayItemID == null)
                            continue;

                        identity++;

                        var secondary = new TrayConsolidationTray()
                        {
                            Identity = identity,
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
                            TotalCases = child.SecondaryCases ?? 0,
                            TrayUsageQty = child.SecondaryUsageQty ?? 0,
                            CardCategory = child.SecondCardCategory,
                            NetEffect = instrumentCount - (child.SecondTrayInstances * trayConsolidation.TrayInstrumentCount)
                        };

                        if (minEffect.HasValue && secondary.NetEffect < minEffect)
                            continue;

                        trayConsolidation.Children.Add(secondary);
                    }

                    identity++;
                }
            }

            return result;
        }

        public static void SummarizeChildren(TrayConsolidationTray grandparent, TrayConsolidationTray parent, List<TrayConsolidation> source)
        {
            parent.Children = new List<TrayConsolidationTray>();

            foreach (var tray in source)
            {
                var consolidation = new TrayConsolidationTray()
                {
                    TrayItemID = tray.SecondTrayItemID ?? -1,
                    TrayName = tray.SecondTray,
                    CardCount = tray.SecondCardCount,
                    RedundantInstruments = tray.RedundantInstruments,
                    OverlapCardCount = tray.OverlapCardCount,
                    OverlapPcnt = tray.OverlapPcnt * 100,
                    TrayInstrumentCount = tray.SecondInstrumentCount,
                    TrayInstances = tray.SecondTrayInstances,
                    TrayAudits = tray.SecondaryAudits ?? 0,
                    TrayCounts = tray.SecondaryCounts ?? 0,
                    TotalCases = tray.SecondaryCases ?? 0,
                    TrayUsageQty = tray.SecondaryUsageQty ?? 0,
                    CardCategory = tray.CardCategory,
                    NetEffect = parent.NetEffect - (tray.SecondTrayInstances * grandparent.TrayInstrumentCount)
                };

                parent.Children.Add(consolidation);
            }
        }
    }

    public class TrayConsolidationTray
    { 
        public int Identity { get; set; }
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
        public int TotalCases { get; set; }
        public int TrayUsageQty { get; set; }
        public string CardCategory { get; set; }
        public int NetEffect { get; set; }
        public List<TrayConsolidationTray> Children { get; set; }

        public decimal TrayOpenPercent => (TotalCases == 0 ? 0.0M : TrayCounts / TotalCases) * 100M;
        public decimal TrayAvgUsage => (TrayCounts == 0 ? 0.0M : TrayUsageQty / TrayCounts);
    }
}
