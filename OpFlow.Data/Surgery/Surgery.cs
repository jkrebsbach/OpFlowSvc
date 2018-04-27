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
        public int? RoomSetupID { get; set; }
        public int? FlowID { get; set; }
        public int UserID { get; set; }
        public int UserRoleID { get; set; }
        public string CaseNumber { get; set; }
        public string SurgeryStatus { get; set; }
        public string ProcedureDescription { get; set; }
        public string BundleDescription { get; set; }
        public string CardDescription { get; set; }
        public string FlowDescription { get; set; }
        public string RoomDescription { get; set; }
        public string SurgeonFirstName { get; set; }
        public string SurgeonLastName { get; set; }
        public string PatientPosition { get; set; }
        public string FlowStepDescription { get; set; }
        public string RoomSetupDescription { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan ScheduleTime { get; set; }
        public DateTime? ActualStartDate { get; set; }
        public TimeSpan? ActualStartTime { get; set; }
        public TimeSpan? DelayStartTime { get; set; }
        public int? EstDelayMinutes { get; set; }
        public int? TotalMinutes { get; set; }
        public string CaseNotes { get; set; }


        public DateTime ScheduleDateTime => ScheduleDate.Add(ScheduleTime);
        public DateTime? ActualStartDateTime => ActualStartTime.HasValue ? ActualStartDate?.Add(ActualStartTime.Value) : null;
        public DateTime? DelayStartDateTime => DelayStartTime.HasValue ? ScheduleDate.Add(DelayStartTime.Value) : (DateTime?)null;
    }

    public class PatientSurgery : Surgery
    {
        public Patient Patient { get; set; }
    }

    public class SurgerySchedule : Surgery
    {
        public List<SurgeryUser> SurgeryUsers { get; set; }
    }

    public class SurgeryPost
    {
        public string CaseNbr { get; set; }
        public string PtAcctNbr { get; set; }
        public DateTime PtDOB { get; set; }
        public string PtInitials { get; set; }
        public string PtGender { get; set; }
        public string PtFirstName { get; set; }
        public string PtLastName { get; set; }
        public decimal PtBMI { get; set; }
        public int SpecialtyID { get; set; }
        public int SurgeonUserID { get; set; }
        public int? RoomID { get; set; }
        public int? BundleID { get; set; }
        public string CptCode { get; set; }
        public DateTime ScheduleDate { get; set; }
    }

    public class SurgeryCountPost
    {
        public List<SurgeryCountItemPost> ItemCounts { get; set; }
        public List<SurgeryCountItemPost> InstrumentCounts { get; set; }
    }

    public class SurgeryCustomItemPost
    {
        public int ItemID { get; set; }
        public int Quantity { get; set; }
        public int? TrayID { get; set; }
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
        public bool Pass1 { get; set; }
        public bool Pass2 { get; set; }
        public bool Pass3 { get; set; }
        public int Usage { get; set; }
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
        public DateTime ScheduleDate { get; set; }
        public TimeSpan ScheduleTime { get; set; }
        public int EstDelayMinutes { get; set; }
        public int CardID { get; set; }
        public int BundleID { get; set; }
        public int ProcedureID { get; set; }
        public string BundleDescription { get; set; }
        public string ProcedureDescription { get; set; }
        public string RoomDescription { get; set; }

        public List<SurgeryUser> SurgeryUsers { get; set; }

        public string SurgeryTeam()
        {
            return SurgeryUsers == null ? null : string.Join(", ", SurgeryUsers.Select(su => su.LastName));
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
        public int TrayItemID { get; set; }
        public int? ItemID { get; set; }
        public int InstrumentID { get; set; }
        public int TrayQuantity { get; set; }
        public int QuantityUsed { get; set; }
        public string TrayName { get; set; }
        public string InstrumentDescription { get; set; }
    }

}