using System;

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
        public int ProcedureID { get; set; }
        public int BundleID { get; set; }
        public int RoomID { get; set; }
        public int FlowID { get; set; }
        public string ProcedureDescription { get; set; }
        public string BundleDescription { get; set; }
        public string CardDescription { get; set; }
        public string FlowDescription { get; set; }
        public string LocationName { get; set; }
        public string SurgeonFirstName { get; set; }
        public string SurgeonLastName { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan ScheduleTime { get; set; }
        public int? EstDelayMinutes { get; set; }
    }
}
