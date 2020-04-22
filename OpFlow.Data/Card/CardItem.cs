using System;
using System.Collections.Generic;
using System.Linq;
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
        public decimal? AvgUsed { get; set; }
        public int? QtyOpenOrig { get; set; }
        public string QtyHoldOrig { get; set; }
        public string QtySource { get; set; }
        public bool DeleteItem { get; set; }
        public bool ValueChanged => QtyOpenOrig != QtyOpen || QtyHoldOrig != QtyHold;
    }

    public class CardItemCountQueryResult
    {
        public List<CardItemCount> CardItemCounts { get; set; }
        public List<CollectionItemCount> TrayCollectionCounts { get; set; }
        public List<TrayQuestion> TrayQuestions { get; set; }
    }

    public class SurgerySutureCount
    {
        public int SutureID { get; set; }
        public int? ItemID { get; set; }
        public string Manufacturer { get; set; }
        public string Size { get; set; }
        public string PackSize { get; set; }
        public int Opened { get; set; }
        public int Used { get; set; }
        public string Notes { get; set; }
    }
    public class CardItemCount : ItemMaster
    {
        public int CardID { get; set; }

        public bool CustomUsage { get; set; }
        public int? Setup { get; set; }
        public int? SetupAdded { get; set; }
        public int? Usage { get; set; }
        public int? UsageType { get; set; }
        public int? RoleID { get; set; }
        public string Notes { get; set; }

        private int SetupQuantity => (Setup ?? 0) + (SetupAdded ?? 0);
        public int? Added {
            get {
                if (Usage == null)
                    return null;

                // Some screens don't have setup - deduct from raw quantity
                if (Setup == null)
                {
                    if (Usage.Value > Quantity)
                        return Usage.Value - Quantity;
                    
                    return null;
                }

                return Usage.Value > SetupQuantity ? Usage.Value - SetupQuantity : 0;
            }
        }
        public int UsageEncoded
        {
            get {
                if (Usage == null)
                    return -1;

                // Some screens don't have setup - deduct from raw quantity
                if (Setup == null)
                {
                    if (Usage.Value > Quantity)
                        return Quantity;

                    return Usage.Value;
                }

                else if (Usage > SetupQuantity)
                    return SetupQuantity;
                else return Usage.Value;
            }
        }
        public int SetupEncoded => Setup ?? -1;

        public static string GetCsvHeader()
        {
            return "Tray,Item,Tray Quantity,Usage\r\n";
        }

        public string GetCsvExport()
        {
            return $"\"{TrayName}\",\"{ItemDescription}\",{Quantity},{Usage}\r\n";
        }
    }

    public class CollectionItemCount : CardItemCount
    {
        public int CollectionItemID { get; set; }
    }

    public class TrayCollection 
    {
        public int ItemID { get; set; }
        public string ItemDescription { get; set; }
        public List<CollectionItemCount> CollectionItems { get; set; }
        public List<TrayQuestion> Questions { get; set; }

    }

    public class SurgeryTrayOpen
    {
        public int SurgeryID { get; set; }
        public int TrayID { get; set; }
        public bool TrayOpened { get; set; }
        public string Feedback { get; set; }
    }

    public class CardItemCountResult
    {
        public Surgery Surgery { get; set; }
        public List<ItemMaster> Sutures { get; set; }
        public List<CardItemCount> Room { get; set; }
        public List<CardItemCount> Supplies { get; set; }
        public List<CardItemCount> Instruments { get; set; }
        public List<SurgerySutureCount> SutureCounts { get; set; }
        public List<TrayCollection> Collections { get; set; }
        public List<TrayUsage> Trays { get; set; }
        public List<SurgeryCPTCode> CptCodes { get; set; }
        public List<TrayUsage> ProposedTrays { get; set; }

        public CardItemCountResult()
        {
            Trays = new List<TrayUsage>();
            ProposedTrays = new List<TrayUsage>();
        }
    }

    public class CardUsageHistory
    {
        public CardSummary Summary { get; set; }
        public List<ItemUsageHistory> TrayItems { get; set; }
        public List<ItemUsageHistory> Supplies { get; set; }
    }

    public class CardCategory
    {
        public int CardCategoryID { get; set; }
        public string CategoryName { get; set; }
    }

    public class ProcedureProfile
    {
        public int ProcedureProfileID { get; set; }
        public string ProcedureProfileName { get; set; }
        public int NbrInstruments { get; set; }
        public int NbrCounts { get; set; }
        public int NbrAudits { get; set; }
        public List<ProfileSpecialty> Specialties { get; set; }
        public List<ProfileProcedure> Procedures { get; set; }
        public List<ProfileItem> Items { get; set; }
        public List<ProfileCard> Cards { get; set; }
        public List<ProfileTray> Trays { get; set; }
        public List<ProfileItem> TrayItems { get; set; }
        public List<ProfileCardCategory> CardCategories { get; set; }
    }

    public class ProcedureProfileTrayUsage
    {
        public string TrayName { get; set; }
        public int CardCount { get; set; }
        public int SurgeonCount { get; set; }
    }

    public class ProfileSpecialty : Specialty
    {
        public int ProcedureProfileID { get; set; }
    }

    public class ProfileCardCategory : CardCategory
    {
        public int ProcedureProfileID { get; set; }
    }

    public class ProfileCard : Card
    {
        public int ProcedureProfileID { get; set; }
        public int NbrInstruments { get; set; }

        public int ProfileCount { get; set; }
        public int CountChange => ProfileCount - NbrInstruments;
        public decimal PcntChange => ProfileCount == 0 ? 0 : ((decimal)CountChange / ProfileCount * 100);
        public int Increase => CountChange > 0 ? 1 : (CountChange < 0 ? -1 : 0);
    }

    public class ProfileTray
    {
        public int ProcedureProfileID { get; set; }
        public string TrayType { get; set; }
        public int ItemID { get; set; }
        public int NbrInstruments { get; set; }
        public string ItemDescription { get; set; }
        public int? TrayProposalID { get; set; }
        public string TrayName { get; set; }

        public int ProfileCount { get; set; }
        public int CountChange => ProfileCount - NbrInstruments;
        public decimal PcntChange => ProfileCount == 0 ? 0 : ((decimal)CountChange / ProfileCount * 100);
        public int Increase => CountChange > 0 ? 1 : (CountChange < 0 ? -1 : 0);
    }

    public class ProfileProcedure : Procedure
    {
        public int ProcedureProfileID { get; set; }
    }

    public class ProfileItem : ItemMaster
    {
        public int ProcedureProfileID { get; set; }
        public string TrayType { get; set; }
        public int NbrInstances { get; set; }
        public int NbrInstruments { get; set; }


        public string CategoryID { get; set; }
        public string Reason { get; set; }
        public int ProfileCount { get; set; }
        public int CountChange => ProfileCount - NbrInstruments;
        public decimal PcntChange => ProfileCount == 0 ? 0 : ((decimal)CountChange / ProfileCount * 100);
        public int Increase => CountChange > 0 ? 1 : (CountChange < 0 ? -1 : 0);

        public List<ComparableItem> ComparableItems { get; set; }
        public List<ComparableInstrument> ComparableInstruments { get; set; }
    }

    public class ProcedureProfilePost
    {
        public string ProfileName { get; set; }
        public List<int> CardCategoryID { get; set; }
        public List<int> SpecialtyID { get; set; }
    }

    public class ProcedureProfileItemUpdatePost
    {
        public List<ProcedureProfileItemPost> Items { get; set; }
    }
    public class ProcedureProfileItemPost
    {
        public string ItemType { get; set; }
        public int ItemID { get; set; }
        public int TrayItemID { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public string Category { get; set; }
        public int CategoryID { get; set; }
    }

    public class ProcedureProfileDashboardPost
    {
        public List<int> Cards { get; set; }
        public List<int> Trays { get; set; }
        public List<int> Proposals { get; set; }
    }

    public class ProcedureProfileCardComparison
    {
        public string ItemType { get; set; }
        public List<CardItem> CardItems { get; set; }
        public List<ProfileItem> ProfileItems { get; set; }
    }

    public class ProcedureProfileDashboardComparisonRequest
    {

        public List<int> SpecialtyID { get; set; }
        public List<int> CardCategoryID { get; set; }
        public List<int> SurgeonID { get; set; }
    }

    public class ProcedureProfileCardComparisonRequest
    {

        public List<int> Cards { get; set; }
        public List<int> Trays { get; set; }
    }

    public class ProcedureProfileAlignmentPost
    {
        public List<int> CardId { get; set; }
        public List<int> TrayId { get; set; }
        public string OrderBy { get; set; }
    }
    public class ProcedureProfileAnalyticsSummary
    {
        public string ProcedureProfile { get; set; }
        public string CardCategory { get; set; }
        public decimal GroupAvgCount { get; set; }
        public decimal GroupAvgCost { get; set; }
        public decimal GroupAvgInstruments { get; set; }
        public decimal ProfileAvgCount { get; set; }
        public decimal ProfileAvgCost { get; set; }
        public decimal ProfileAvgInstruments { get; set; }
    }

    public class ProcedureProfileDashboardGroup
    {

        public string TrayName { get; set; }
        public string InstrumentName { get; set; }
        public int TrayQuantity => Results.First().TrayQuantity;
        public int NetUsage => Results.Sum(r => r.NetUsage);
        public int MaxUsage => Results.Max(r => r.MaxUsage);
        public decimal AvgUsage => NetUsage == 0 ? 0 : (decimal)NetUsage / Results.Sum(r => r.CaseCount);
        public string Category => Results.First().Category;
        public string Reason => Results.First().Reason;

        public List<ProcedureProfileDashboardComparison> Results { get; set; }
    }

    public class ProcedureProfileDashboardComparison
    {
        public string GroupName { get; set; }
        public string TrayName { get; set; }
        public string InstrumentName { get; set; }
        public int TrayQuantity { get; set; }
        public int NetUsage { get; set; }
        public int MaxUsage { get; set; }
        public int CaseCount { get; set; }
        public string Reason { get; set; }
        public string Category { get; set; }

        public decimal AvgUsage => NetUsage == 0 ? 0 : (decimal)NetUsage / CaseCount;

        public static List<ProcedureProfileDashboardComparison> Summarize(IEnumerable<string> categories, IEnumerable<ProcedureProfileDashboardComparison> data)
        {
            var result = new List<ProcedureProfileDashboardComparison>();

            foreach (var category in categories)
            {
                result.Add(data.FirstOrDefault(d => d.GroupName == category) ?? new ProcedureProfileDashboardComparison());
            }

            return result;
        }
    }

    public class TrayGroup
    {
        public int? TrayGroupID { get; set; }
        public string GroupName { get; set; }
        public List<TrayGroupTray> Trays { get; set; }
    }
    public class TrayGroupTray
    {
        public int TrayGroupID { get; set; }
        public int TrayItemID { get; set; }
        public string TrayName { get; set; }
    }


    public class CardWithCategory : Card
    {
        public List<CardCategoryXRef> CardCategories { get; set; }
    }

    public class CardCategoryXRef
    {
        public int CardCategoryXrefID { get; set; }
        public int CardID { get; set; }
        public int CardCategoryID { get; set; }
        public string HierarchyLevel { get; set; }
        public string CardCategory { get; set; }
    }

    public class UpdateCardCategoryPost
    {
        public string HierarchyLevel { get; set; }
        public string CardCategory { get; set; }
        public List<int> Cards { get; set; }
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

    public class UsedCardSearchPost
    {
        public List<int> UserIDs { get; set; }
        public List<int> TrayIDs { get; set; }
        public List<int> CategoryIDs { get; set; }
    }

    public class UsedInstrumentSearchPost
    {
        public List<int> CardIDs { get; set; }
    }
}
