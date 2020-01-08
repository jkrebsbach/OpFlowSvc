using System;
using System.Collections.Generic;
using System.Linq;

namespace OpFlow.Data
{
    public class Surgery
    {
        public int SurgeryID { get; set; }
        public int SpecialtyID { get; set; }
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public int PatientID { get; set; }
        public int? CardID { get; set; }
        public int CaseID { get; set; }
        public int? ProcedureID { get; set; }
        public int? BundleID { get; set; }
        public int? RoomID { get; set; }
        public int? LateralityID { get; set; }
        public int? RoomSetupID { get; set; }
        public int? FlowID { get; set; }
        public int? CaseProfileID { get; set; }
        public int UserID { get; set; }
        public int UserRoleID { get; set; }
        public int? StepID { get; set; }
        public string CaseNumber { get; set; }
        public string CurrentStep { get; set; }
        public string SurgeryStatus { get; set; }
        public string StaffChange { get; set; }
        public string ProcedureDescription { get; set; }
        public string BundleDescription { get; set; }
        public string CardDescription { get; set; }
        public string LateralityDescription { get; set; }
        public string FlowDescription { get; set; }
        public string RoomDescription { get; set; }
        public string SurgeonFirstName { get; set; }
        public string SurgeonLastName { get; set; }
        public string PatientPosition { get; set; }
        public string FlowStepDescription { get; set; }
        public string CaseProfile { get; set; }
        public string RoomSetupDescription { get; set; }
        public int SharpCount { get; set; }
        public int NeedleCount { get; set; }
        public int LapCount { get; set; }
        public int SpecimenCount { get; set; }
        public string CountComments { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan ScheduleTime { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public TimeSpan? ActualStartTime { get; set; }
        public TimeSpan? DelayStartTime { get; set; }
        public TimeSpan? EstFinishTime { get; set; }
        public int? EstDelayMinutes { get; set; }
        public int? TotalMinutes { get; set; }
        public string CaseNotes { get; set; }


        public DateTime ScheduleDateTime => ScheduleDate.Add(ScheduleTime);
        public DateTime? ActualStartDateTime => ActualStartTime.HasValue ? ActualStartDate?.Add(ActualStartTime.Value) : null;
        public DateTime? DelayStartDateTime => DelayStartTime.HasValue ? ScheduleDate.Add(DelayStartTime.Value) : (DateTime?)null;
        public DateTime? EstFinishDateTime => EstFinishTime.HasValue ? ScheduleDate.Add(EstFinishTime.Value) : (DateTime?)null;
    }

    public class RoomOverview
    {
        public int RoomID { get; set; }
        public string RoomDescription { get; set; }
        public List<RoomHour> RoomHours { get; set; }
    }

    public class RoomHour
    {
        public List<RoomSummary> RoomSummary { get; set; }
        
        private RoomHour(IEnumerable<RoomSummary> summary)
        {
            RoomSummary = summary.ToList();
        }
        
        public static List<RoomHour> Summarize(List<RoomSummary> roomSummary)
        {
            var result = new List<RoomHour>();

            var startTime = 7;
            var endTime = 22;

            result.Add(new RoomHour(roomSummary.Where(rs => rs.ScheduleTime.Hours < startTime)));

            while (startTime <= endTime)
            {
                result.Add(new RoomHour(roomSummary.Where(rs => rs.ScheduleTime.Hours == startTime)));
                startTime++;
            }

            result.Add(new RoomHour(roomSummary.Where(rs => rs.ScheduleTime.Hours > endTime)));

            return result;
        }
    }

    public class RoomSummary : Surgery
    {
        public int? IdleMinutes { get; set; }
        public bool AuditSurgery { get; set; }
        public bool CountSurgery { get; set; }

        public Patient Patient { get; set; }
    }

    public class PatientSurgery : Surgery
    {
        public Patient Patient { get; set; }
    }
    
    public class NewSurgerySetup
    {
        public List<Room> Rooms { get; set; }   
        public List<Specialty> Specialties { get; set; }
        public List<Laterality> Lateralities { get; set; }
        public List<User> Surgeons { get; set; }
    }

    public class NewSurgeryVendor
    {
        public List<OpFlowProvider> Providers { get; set; }
        public List<CaseProfile> CaseProfiles { get; set; }
        public List<TrayGroup> TrayGroups { get; set; }
    }

    public class SurgerySchedule : Surgery
    {
        public List<SurgeryUser> SurgeryUsers { get; set; }
    }

    public class SurgeryPost
    {
        public int? VendorLocationID { get; set; }
        public string CaseNbr { get; set; }
        public string PtAcctNbr { get; set; }
        public DateTime? PtDOB { get; set; }
        public string PtGender { get; set; }
        public string PtFirstName { get; set; }
        public string PtLastName { get; set; }
        public string PtMiddleInitial { get; set; }
        public decimal? PtBMI { get; set; }
        public int? SpecialtyID { get; set; }
        public int? SurgeonUserID { get; set; }
        public int? RoomID { get; set; }

        public int? BundleID { get; set; }
        public int? CardID { get; set; }
        public int? LateralityID { get; set; }
        public int? CaseProfileID { get; set; }
        public int? SurgeonPreferenceID { get; set; }
        public List<int> SecondarySurgeons { get; set; }
        public List<int> TrayGroupID { get; set; }
        public string CptCode { get; set; }
        public DateTime ScheduleDate { get; set; }
    }

    public class SurgeryCountPost
    {
        public List<SurgeryCountItemPost> ItemCounts { get; set; }
        public List<SurgeryCountItemPost> InstrumentCounts { get; set; }
        public List<SurgeryCountItemPost> ProposedCounts { get; set; }
        public List<SutureCountItemPost> SutureCounts { get; set; }
        public List<int> DeletedSutures { get; set; }
        public List<TrayQuestion> Answers { get; set; }
        public List<string> SurgeryCpts { get; set; }
        public string CountComments { get; set; }
    }

    public class SurgeryCPTCode
    {
        public int SurgeryID { get; set; }
        public string CptCode { get; set; }
    }

    public class SurgeryCustomItemPost
    {
        public List<NewSurgeryCustomItem> Items { get; set; }
    }

    public class NewSurgeryCustomItem
    {
        public int ItemID { get; set; }
        public int Quantity { get; set; }
        public int? TrayID { get; set; }
    }
    public class SurgeryTeamUpdate
    {
        public List<int> Surgeries { get; set; }
        public List<SurgeryTeamEdit> Edits { get; set; }
    }

    public class SurgeryTeamEdit
    {
        public int UserID { get; set; }
        public bool Assign { get; set; }
    }

    public class SurgeryProcedureEditPost
    {
        public bool IsPerformed { get; set; }
    }

    public class SurgeryEditPost
    {
        public int RoomID { get; set; }
        public DateTime ScheduleDateTime { get; set; }
        public int? NotificationUser { get; set; }
    }
    public class SurgeryCountItemPost
    {
        public int ItemID { get; set; }
        public int TrayID { get; set; }
        public int? RoleID { get; set; }
        public int? UsageType { get; set; }
        public int? Setup { get; set; }
        public int? SetupAdded { get; set; }
        public int Usage { get; set; }
    }
    public class SutureCountItemPost
    {
        public int? SutureID { get; set; }
        public int? ItemID { get; set; }
        public string Manufacturer { get; set; }
        public string Size { get; set; }
        public string PackSize { get; set; }
        public int? SetupAdded { get; set; }
        public int Usage { get; set; }
    }

    public class SurgeryDelay
    {
        public int SurgeryID { get; set; }
        public int TotalMinutes { get; set; }
        public decimal DelayCost { get; set; }
        public int ReasonID { get; set; }
        public string ReasonDescription { get; set; }
    }

    public class SurgeryDelayReason
    {
        public int ReasonID { get; set; }
        public string ReasonDescription { get; set; }
    }

    public class SurgeryDelayPost
    {
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public int? DelayReasonID { get; set; }
    }

    public class SurgerySearchResult
    {
        public int UserID { get; set; }
        public int SurgeryID { get; set; }
        public string SurgeryStatus { get; set; }
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public int PatientID { get; set; }
        public int RoomID { get; set; }
        public int LateralityID { get; set; }
        public DateTime ScheduleTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? EstimatedCompletionTime { get; set; }
        public DateTime? ActualCompletionTime { get; set; }
        public int EstDelayMinutes { get; set; }
        public int? TotalMinutes { get; set; }
        public int CardID { get; set; }
        public int BundleID { get; set; }
        public int ProcedureID { get; set; }
        public string CPTCode { get; set; }
        public string BundleDescription { get; set; }
        public string CardDescription { get; set; }
        public string ProcedureDescription { get; set; }
        public string RoomDescription { get; set; }
        public string FlowStepDescription { get; set; }
        public string LateralityDescription { get; set; }
        public string StaffChange { get; set; }
        public bool AuditSurgery { get; set; }
        public bool CountSurgery { get; set; }

        public List<SurgeryUser> SurgeryUsers { get; set; }

        public string SurgeryTeam
        {
            get
            {
                return SurgeryUsers == null ? null : string.Join(", ", SurgeryUsers.Select(su => su.LastName));
            }
        }

        public SurgerySearchResult()
        {
            SurgeryUsers = new List<SurgeryUser>();
        }
    }

    public class SurgeryUtilizationCount
    {
        public List<SurgeryItemCount> SurgeryItemCounts { get; set; }
        public List<SurgeryInstrumentCount> SurgeryInstrumentCounts { get; set; }
    }

    public class SurgeryItemCount
    {
        public int SurgeryID { get; set; }
        public int ItemID { get; set; }
        public int QuantityWasted { get; set; }
        public string ItemDescription { get; set; }
    }

    public class SurgeryInstrumentCount
    {
        public int SurgeryID { get; set; }
        public int? TrayItemID { get; set; }
        public int? ItemID { get; set; }
        public int? InstrumentID { get; set; }
        public int TrayQuantity { get; set; }
        public int QuantityUsed { get; set; }
        public string TrayName { get; set; }
        public string InstrumentDescription { get; set; }
    }

}