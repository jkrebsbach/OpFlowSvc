using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpFlow.Data
{

    public class SurgeryAudits
    {
        public List<User> ScrubTechs { get; set; }
        public List<SurgeryTrayAudit> Audits { get; set; }
    }

    public class SurgeryTrayAudit : TrayRationalization
    {
        public string ScrubTechUser { get; set; }
        public string AuditUser { get; set; }
        public List<TrayRationalizationItem> Instruments { get; set; }
    }

    public class TrayRationalization
    {
        public int TrayProposalID { get; set; }
        public int? VendorID { get; set; }
        public int? TrayProposalPhaseID { get; set; }
        public int? SpecialtyID { get; set; }
        public int? TrayOwnerUserID { get; set; }
        public bool CustomizedTray { get; set; }
        public string Vendor { get; set; }
        public string Status { get; set; }
        public string StatusName { get; set; }
        public string DeploymentStatus { get; set; }
        public string TrayName { get; set; }
        public string TrayProposalPhase { get; set; }
        public string Specialty { get; set; }
        public int InstrumentCount { get; set; }
        public int Instances { get; set; }
        public int Counts { get; set; }
        public int Audits { get; set; }
        public int CountsScheduled { get; set; }
        public int AuditsScheduled { get; set; }
        public decimal ReductionPcnt { get; set; }
        public decimal ReductionCount { get; set; }
        public DateTime? CountCompleteTarget { get; set; }
        public DateTime? AuditCompleteTarget { get; set; }
        public DateTime? TrayChangesTarget { get; set; }
        public string ImageFilename { get; set; }
        public string OrgChart { get; set; }
        public string TrayOwner { get; set; }
        public string Comments { get; set; }
        public int ProposedCount { get; set; }
        public int SourceCount { get; set; }
        public int CountChange => SourceCount - ProposedCount;
        public int ProjectRemoval => CountChange * Instances;
        public decimal PcntChange => SourceCount == 0 ? 0 : ((decimal)CountChange / SourceCount * 100);

        public List<TrayProposalUserAssignment> UserAssignments { get; set; }
        public List<TrayProposalUserAssignment> TrayApprovers => UserAssignments?.Where(ua => ua.UserType == "A")?.ToList();
        public List<TrayProposalUserAssignment> TrayUsers => UserAssignments?.Where(ua => ua.UserType == "U")?.ToList();
        public List<TrayApproval> TrayApprovals { get; set; }
        public string DeploymentStatusName
        {
            get
            {

                var result = string.Empty;
                switch (DeploymentStatus)
                {
                    case "A":
                        return "Ahead";
                    case "B":
                        return "Behind";
                    case "H":
                        return "Hold";
                    case "C":
                        return "Completed";
                    default:
                        return "On Time";
                }
            }
        }
    }
    public class TrayProposalUserAssignment : User
    { 
        public int TrayProposalID { get; set; }
        public string UserType { get; set; }
    }
    public class TrayRationalizationCardCategory
    {
        public int TrayProposalID { get; set; }
        public int CardCategoryID { get; set; }
    }

    public class TrayProposalTrayGroup : TrayGroup
    {
        public int TrayProposalID { get; set; }
    }

    public class TrayApproval
    {
        public int DocumentTypeID { get; set; }
        public string ApprovalDescription { get; set; }
        public string ApprovalFilename { get; set; }
        public DateTimeOffset? ApprovalUpload { get; set; }
    }

    public class SourceTraySummary
    {
        public int TrayItemID { get; set; }
        public string TrayName { get; set; }
        public string CardCategories { get; set; }
        public int CountsComplete { get; set; }
        public int CountsScheduled { get; set; }
        public int AuditsComplete { get; set; }
        public int AuditsScheduled { get; set; }

        public int InstrumentCount { get; set; }
        public int ProposedInstrumentCount { get; set; }
        public int CountChange => InstrumentCount - ProposedInstrumentCount;
        public decimal PcntChange => InstrumentCount == 0 ? 0 : ((decimal)CountChange / InstrumentCount * 100);

        public List<ItemTrayOverlap> Instruments { get; set; }
    }

    public class TrayProposalPhase
    {
        public int TrayProposalPhaseID { get; set; }
        public string TrayProposalPhaseDescription { get; set; }
    }

    public class TrayRationalizationItem : ItemTrayOverlap
    {
        public int TrayProposalID { get; set; }
        public int? CurrentInstrumentID { get; set; }
        public int InstrumentCount { get; set; }
        public decimal AvgPerCase { get; set; }
        public int SourceQty { get; set; }
        public int? TrayProposalCategoryID { get; set; }
        public int? EponymID { get; set; }
        public int? TypeID { get; set; }
        public string Description { get; set; }
        public string Size { get; set; }
        public int TrayCases { get; set; }
        public int UsedCases { get; set; }
        public string Reason { get; set; }
        public string Comments { get; set; }
        public string ConfigurationNotes { get; set; }
        public string HistoryType { get; set; }
        public int Proposed { get; set; }
        public DateTimeOffset? UpdTimestamp { get; set; }

        public decimal CaseUsagePcnt => TrayCases == 0 ? 0 : (UsedCases / (decimal)TrayCases * 100);

        public string HighlightClass => (SourceQty - Quantity == 0 ? "" : "highlight-tray");
    }

    public class TrayRationalizationStatusLog
    {
        public string Status { get; set; }
        public string Phase { get; set; }
        public string StatusName { get; set; }
        public string StatusUser { get; set; }
        public DateTimeOffset StatusDate { get; set; }

    }

    public class TrayRationalizationCard
    {
        public string SurgeonName { get; set; }
        public string CardName { get; set; }
        public string SpecialtyName { get; set; }
        public string SourceTrayName { get; set; }
        public string NewTrayName { get; set; }
        public string InstrumentName { get; set; }
        public int Proposed { get; set; }
    }

    public class TrayRationalizationConfigPost
    {
        public List<int> Specialties { get; set; }
        public List<int> Trays { get; set; }
        public List<int> Surgeons { get; set; }
        public List<int> Cards { get; set; }
        public List<TrayQuestion> Questions { get; set; }
        public string CptCode { get; set; }
    }

    public class CardListPost
    {
        public List<CardListTrayPost> Trays { get; set; }
    }

    public class CardListTrayPost
    {
        public int CardID { get; set; }
        public int TrayID { get; set; }
    }

    public class ProposedTrayPost
    {
        public int? VendorID { get; set; }
        public int? PhaseID { get; set; }
        public int? SpecialtyID { get; set; }
        public bool Customized { get; set; }
        public string TrayName { get; set; }
        public string Status { get; set; }
        public List<ProposedTrayInstrumentPost> Instruments { get; set; }
        public List<int> CardCategories { get; set; }
        public List<string> TrayGroups { get; set; }
    }

    public class TrayGroupPost
    {
        public List<TrayGroup> TrayGroups { get; set; }
    }

    public class ProposedTrayDashboardPost
    {
        public string DeploymentStatus { get; set; }
        public int Instances { get; set; }
        public DateTime? CountComplete { get; set; }
        public DateTime? AuditComplete { get; set; }
        public DateTime? TrayChanges { get; set; }
        public string Comments { get; set; }
    }

    public class ProposedTrayInstrumentPost
    {
        public int InstrumentID { get; set; }
        public int? TrayItemID { get; set; }
        public int Quantity { get; set; }
    }

    public class ProposedTrayUpdatePost
    {
        public List<UpdateTrayInstrumentPost> Instruments { get; set; }
    }

    public class UpdateTrayInstrumentPost
    {
        public int InstrumentID { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; }
        public int? CategoryID { get; set; }
        public int? EponymID { get; set; }
        public int? TypeID { get; set; }
        public string Description { get; set; }
        public string Size { get; set; }
        public string Comments { get; set; }
        public string Notes { get; set; }
        public int Sequence { get; set; }
    }

    public class AuditDetailPost
    {
        public List<UpdateAuditDetail> Audits { get; set; }
    }

    public class UpdateAuditDetail
    {
        public int SurgeryID { get; set; }
        public List<string> SurgeryCpts { get; set; }
        public string Comments { get; set; }
    }

    public class TrayRationalizationComparePost
    {
        public List<TrayRationalizationCompareDetail> Comparisons { get; set; }
    }

    public class TrayRationalizationCompareDetail
    {
        public int CustomerID { get; set; }
        public int BaselineID { get; set; }
    }

    public class TrayRationalizationCompareResult
    {
        public int CustomerQuantity => Instruments.Sum(i => i.CustomerQuantity);
        public int BaselineQuantity => Instruments.Sum(i => i.BaselineQuantity);
        public List<TrayRationalizationCompare> Instruments { get; set; }
        public List<TrayRationalizationCompareCategory> CustomerCategories { get; set; }
        public List<TrayRationalizationCompareCategory> BaselineCategories { get; set; }
    }

    public class TrayRationalizationCompareCategory
    {
        public int TrayProposalID { get; set; }
        public string CardCategory { get; set; }
        public int CategorySum { get; set; }
    }

    public class TrayRationalizationCompareResultSummary
    {
        public int TrayCompareCount { get; set; }
        public int CustomerCount { get; set; }
        public int BaselineCount { get; set; }
        public decimal PercentReduction => BaselineCount == CustomerCount ? 0 : (1 - ((decimal)CustomerCount / BaselineCount)) * 100;

        public static TrayRationalizationCompareResultSummary SummarizeResults(List<TrayRationalizationCompareResult> results)
        {
            var summary = new TrayRationalizationCompareResultSummary();

            summary.TrayCompareCount = results.Count;
            summary.CustomerCount = results.Sum(r => r.CustomerQuantity);
            summary.BaselineCount = results.Sum(r => r.BaselineQuantity);

            return summary;
        }
    }


    public class TrayRationalizationCompare
    {
        public string CustomerTrayName { get; set; }
        public string BaselineTrayName { get; set; }
        public string InstrumentCategory { get; set; }
        public int UsedInstruments { get; set; }
        public int CurrentCards { get; set; }
        public int CustomerQuantity { get; set; }
        public int BaselineQuantity { get; set; }
        public int CommonInstruments { get; set; }
    }

    public class TrayRationalizationSummary
    {
        public int? TrayItemID { get; set; }
        public string TrayName { get; set; }
        public int Quantity { get; set; }
        public int SourceQty { get; set; }
        public decimal Ratio => 100 - (SourceQty == 0 ? 0 : (decimal)Quantity / SourceQty * 100);
    }

    public class TrayRationalizationDetailPost
    {
        public int TrayProposalID { get; set; }
        public string Rollup { get; set; }
        public List<TrayDetailPost> TrayIDs { get; set; }
    }

    public class TrayDetailResult
    {
        public List<TrayRationalizationItem> Instruments { get; set; }
    }

    public class TrayRationalizationDetailItem
    {
        public TrayRationalizationItem ProposedInstrument { get; set; }
        public List<TrayRationalizationItem> SourceInstruments { get; set; }
        public List<TrayRationalizationDetail> TrayInstruments { get; set; }

        public TrayRationalizationDetailItem()
        {
            SourceInstruments = new List<TrayRationalizationItem>();
            TrayInstruments = new List<TrayRationalizationDetail>();
        }
    }

    public class TrayDetailPost
    {
        public int ID { get; set; }
        public string Type { get; set; }
    }

    public class TrayRationalizationOverlapPost
    {
        public List<string> Trays { get; set; }
        public int? StandardizedTrayID { get; set; }
    }

    public class TrayRationalizationDetailResult
    {
        public string TrayName { get; set; }
        public int Quantity { get; set; }
        public List<TrayRationalizationDetail> Instruments { get; set; }
    }

    public class TrayRationalizationDetail
    {
        public string SurgeonName { get; set; }
        public string CardName { get; set; }
        public string TrayName { get; set; }
        public int InstrumentID { get; set; }
        public string InstrumentName { get; set; }
        public string InstrumentCategory { get; set; }
        public int QtyOpen { get; set; }
        public decimal AvgUsed { get; set; }
        public int ProposedQty { get; set; }
    }

    public class TrayCardOverlapSummary
    {
        public string TrayName { get; set; }
        public int NbrInstances { get; set; }
        public int NbrInstruments { get; set; }
        public decimal CostPerTray { get; set; }
        public decimal CostAllTrays { get; set; }
        public int ProcessedAvg { get; set; }
        public int ProcessedMin { get; set; }
        public int ProcessedMax { get; set; }
        public decimal UsedInstruments { get; set; }
        public int CommonInstruments { get; set; }
        public decimal? OverlapPcnt =>
            (CommonInstruments == 0 ? 0 : (decimal)UsedInstruments / CommonInstruments * 100);
    }

    public class InstrumentLookup
    {
        public List<TrayInstrumentCategory> Categories { get; set; }
        public List<TrayInstrumentEponym> Eponyms { get; set; }
        public List<TrayInstrumentType> Types { get; set; }
    }
    
    public class TrayInstrumentCategory
    {
        public int CategoryID { get; set; }
        public int SpecialtyID { get; set; }
        public string CategoryDescription { get; set; }
    }

    public class TrayInstrumentEponym
    {
        public int EponymID { get; set; }
        public string EponymDescription { get; set; }
    }

    public class TrayInstrumentType
    {
        public int TypeID { get; set; }
        public string TypeDescription { get; set; }
    }

    public class CaseProfile
    {
        public int CaseProfileID { get; set; }
        public string CaseProfileType { get; set; }
        public string CaseProfileName { get; set; }
        public string CaseProfileTypeName => CaseProfileType == "S" ? "Schedule" : "Perioperative";

        public List<CaseProfileQuestion> Questions { get; set; }

        public CaseProfile()
        {
            Questions = new List<CaseProfileQuestion>();
        }

        public void ParseResults(IEnumerable<CaseProfileQuestionResult> questions)
        {
            foreach (var questionAnswer in questions.OrderBy(q => q.QuestionID))
            {
                var question = Questions.FirstOrDefault(q => q.QuestionID == questionAnswer.QuestionID);
                if (question == null)
                {
                    question = new CaseProfileQuestion()
                    {
                        QuestionID = questionAnswer.QuestionID,
                        Question = questionAnswer.Question,
                        AnswerID = questionAnswer.QuestionAnswerID
                    };

                    Questions.Add(question);
                }

                if (questionAnswer.AnswerID.HasValue)
                {
                    var answer = new CaseProfileAnswer()
                    {
                        AnswerID = questionAnswer.AnswerID,
                        Answer = questionAnswer.Answer
                    };

                    question.Answers.Add(answer);
                }
            }
        }
    }

    public class CaseProfileQuestionResult
    {
        public int CaseProfileID { get; set; }
        public int QuestionID { get; set; }
        public int? AnswerID { get; set; }
        public string Question { get; set; }
        public int? QuestionAnswerID { get; set; }
        public string Answer { get; set; }
    }

    public class CaseProfilePost
    {
        public string ProfileName { get; set; }
        public string ProfileType { get; set; }
        public List<CaseProfileQuestion> Questions { get; set; }
    }
    public class ScheduleRulePost
    {
        public List<int> Surgeon { get; set; }
        public List<int> Category { get; set; }
        public DateTime? BeginDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class TrayProposalRepPost
    {
        public bool Ignore { get; set; }
    }
    public class TraySchedulePost
    {
        public List<ScheduleTrayPost> Trays { get; set; }
        public List<CaseProfileSchedulePost> CaseProfiles { get; set; }
    }
    public class ScheduleTrayPost
    {
        public int? TrayProposalID { get; set; }
        public int? TrayGroupID { get; set; }
    }

    public class TrayScheduleDetailPost
    {
        public string Supplies { get; set; }
    }

    public class TrayCommunicationRolePost
    {
        public int? OwnerID { get; set; }
        public List<int> Approvers { get; set; }
        public List<int> Users { get; set; }
    }

    public class CaseProfileSchedulePost
    {
        public int CaseProfileID { get; set; }
        public List<CaseProfileQuestionPost> Questions { get; set; }
    }

    public class CaseProfileQuestionPost
    {
        public int QuestionID { get; set; }
        public int? AnswerID { get; set; }
    }

    public class CaseProfileQuestion
    {
        public int? QuestionID { get; set; }
        public string Question { get; set; }

        public int? AnswerID { get; set; }
        public List<CaseProfileAnswer> Answers { get; set; }

        public CaseProfileQuestion()
        {
            Answers = new List<CaseProfileAnswer>();
        }
    }

    public class CaseProfileAnswer
    {
        public int? AnswerID { get; set; }
        public string Answer { get; set; }
    }


    public class TrayCardOverlap
    {
        public int CardID { get; set; }
        public int TrayID { get; set; }
        public string SpecialtyName { get; set; }
        public string CardDescription { get; set; }
        public string SurgeonName { get; set; }
        public int TimesUsed { get; set; }
        public int CountsComplete { get; set; }
        public int AuditsComplete { get; set; }
        public string TrayName { get; set; }
        public int CoveredInstruments { get; set; }
        public decimal CurrentTrayItems { get; set; }
        public decimal UsedInstruments { get; set; }
        public int CommonInstruments { get; set; }
        public decimal MissingInstruments { get; set; }
        public bool ReplaceCard { get; set; }
        //public decimal Overlap => (decimal)CoveredInstruments / CurrentTrayItems * 100;
        public decimal? OverlapPcnt =>
            (CurrentTrayItems == 0 ? 0 : UsedInstruments / CurrentTrayItems * 100);

        public bool AllIncluded => MissingInstruments <= 0;
    }

    public class TrayCountSummary
    {
        public string TrayName { get; set; }
        public string Surgeon { get; set; }
        public int Times { get; set; }
        public string Procedure { get; set; }
        public string CptCodes { get; set; }
        public DateTime SurgeryDate { get; set; }
        public int QtyUsed { get; set; }
    }

    public class TraySurgeryAudit
    {
        public int TrayProposalID { get; set; }
        public int SurgeryID { get; set; }
        public int SpecialtyID { get; set; }
        public int? AuditUserID { get; set; }
        public int? ScheduleID { get; set; }
        public string ProposedTrayName { get; set; }
        public string SurgeonName { get; set; }
        public string RoomDescription { get; set; }
        public DateTime? ScheduleTime { get; set; }
        public string ScrubTechUser { get; set; }
        public string AuditUser { get; set; }
        public string TrayStatus { get; set; }
        public string ResearchStatus { get; set; }
        public string TrayProposalPhase { get; set; }
        public string AuditComments { get; set; }
        public string AuditType { get; set; }
        public string Procedure { get; set; }
        public int? UsedInstruments { get; set; }
        public int SurgeonAuditCount { get; set; }

        public string CptCode1 => CptCodes.Count > 0 ? CptCodes[0]?.CptCode : null;
        public string CptCode2 => CptCodes.Count > 1 ? CptCodes[1]?.CptCode : null;
        public string CptCode3 => CptCodes.Count > 2 ? CptCodes[2]?.CptCode : null;

        public List<SurgeryUser> ScrubTechs { get; set; }
        public List<SurgeryAuditSourceTray> SourceTrays { get; set; }
        public List<SurgeryCPTCode> CptCodes { get; set; }

        public TraySurgeryAudit()
        {
            ScrubTechs = new List<SurgeryUser>();
            SourceTrays = new List<SurgeryAuditSourceTray>();
            CptCodes = new List<SurgeryCPTCode>();
        }
    }

    public class AdminTrayProposal : TrayRationalization
    {
        public int ProviderID { get; set; }
        public int LocationID { get; set; }

        public List<TrayProposalRep> ProposalUsers { get; set; }
    }

    public class TrayProposalRep : User
    {
        public int TrayProposalID { get; set; }
    }

    public class TrayProposalSchedule
    {
        public int ScheduleID { get; set; }
        public int? TrayProposalID { get; set; }
        public int? TrayGroupID { get; set; }
        public int SurgeryID { get; set; }
        public bool CustomizedTray { get; set; }
        public DateTime ScheduleTime { get; set; }
        public DateTime? TrayChangesTarget { get; set; }
        public DateTimeOffset? LatestMessage { get; set; }
        public string RoomDescription { get; set; }
        public string Surgeon { get; set; }
        public string CardDescription { get; set; }
        public string TrayName { get; set; }
        public string TrayGroup { get; set; }
        public string CommunicationStatusID { get; set; }
        public string Supplies { get; set; }
        public bool Ignore { get; set; }

        public string ScheduleTimeDelta
        {
            get
            {
                var delta = ScheduleTime.Subtract(DateTime.Today).Days;

                return delta > 0 ? $"(+{delta} days)" : $"({delta} days)";
            }
        }
        public string TrayChangesDelta
        {
            get
            {
                if (TrayChangesTarget == null)
                    return string.Empty;

                var delta = TrayChangesTarget.Value.Subtract(DateTime.Today).Days;
                return delta > 0 ? $"(+{delta} days)" : $"({delta} days)";
            }
        }

        public string CommunicationStatus {
            get
            {
                switch (CommunicationStatusID)
                {
                    case "S":
                        return "Submitted";
                    case "A":
                        return "Accepted";
                    case "H":
                        return "Hold";
                    case "B":
                        return "Built";
                    case "T":
                        return "Transport";
                    case "AV":
                        return "Available";
                    default:
                        return "Pending";
                }
            }
        }

        public bool ScheduleSubmitted => CommunicationStatus != "Pending";

        public bool Assigned { get; set; }

        public List<ProposalCardCategory> CardCategories { get; set; }
        public List<SurgeryUser> SurgeryUsers { get; set; }
        
        public TrayProposalSchedule()
        {
            CardCategories = new List<ProposalCardCategory>();
            SurgeryUsers = new List<SurgeryUser>();
        }
        public string SurgeryTeam
        {
            get
            {
                return SurgeryUsers == null ? null : string.Join(", ", SurgeryUsers.Select(su => su.LastName));
            }
        }
    }

    public class ProposalCardCategory : CardCategory 
    {
        public int TrayProposalID { get; set; }
    }
    public class TrayProposalScheduleRule
    {
        public string Surgeon { get; set; }
        public string Category { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class TrayRationalizationExport : TrayRationalizationItem
    {
        public int ProposedQuantity { get; set; }
        public int SourceQuantity { get; set; }
    }

    public class ProposedTrayExport
    {
        public List<TrayRationalizationExport> ProposedInstruments { get; set; }
        public List<TrayRationalizationExport> SourceInstruments { get; set; }
    }

    public class SurgeryAuditSourceTray
    {
        public int TrayID { get; set; }
        public int TrayProposalID { get; set; }
        public int SurgeryID { get; set; }
        public string TrayName { get; set; }
    }

    public class SurgeryAuditSearchResult : SurgerySearchResult
    {
        public string SurgeonName { get; set; }
        public int? AuditID { get; set; }

        public List<SurgeryAuditTray> Trays { get; set; }

        public SurgeryAuditSearchResult()
        {
            Trays = new List<SurgeryAuditTray>();
        }
    }

    public class SurgeryAuditTray
    {
        public int SurgeryID { get; set; }
        public string TrayName { get; set; }
    }

    public class AddCaseAuditPost
    {
        public List<int> Surgeries { get; set; }
        public string Target { get; set; }
    }

    public class CommunicationStatusPost
    {
        public string Status { get; set; }
    }

    public class CommunicationPost
    {
        public List<int> Users { get; set; }
    }

    public class TrayRationalizationReduction
    {
        public string TrayName { get; set; }
        public int TrayQuantity { get; set; }
        public decimal AvgUsage { get; set; }
        public decimal AvgUtilization => TrayQuantity == 0 ? 0 : AvgUsage / TrayQuantity;
    }

    public class TrayRationalizationUsage
    {
        public int InstrumentID { get; set; }
        public string InstrumentName { get; set; }
        public string Category { get; set; }
        public decimal AvgUsed { get; set; }
        public List<TrayRationalizationUsageDetail> Details { get; set; }
    }

    public class TrayRationalizationUsageDetail
    {
        public int InstrumentID { get; set; }
        public int TrayItemID { get; set; }
        public string TrayName { get; set; }
        public int TrayQuantity { get; set; }
        public decimal AvgUsed { get; set; }
        public bool Main { get; set; }
        public bool AddOn { get; set; }
        public bool Single { get; set; }
        public bool Peel { get; set; }
    }
}
