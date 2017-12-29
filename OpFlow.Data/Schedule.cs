using System;

namespace OpFlow.Data
{
    public class Schedule
    {
        public int SurgeryID { get; set; }
        public int SpecialtyID { get; set; }
        public int ProviderID { get; set; }
        public int LocationID { get; set; }
        public int PatientID { get; set; }
        public int CardID { get; set; }
        public int BundleID { get; set; }
        public int ProcedureID { get; set; }
        public string ProviderName { get; set; }
        public string SurgeonFirstName { get; set; }
        public string SurgeonLastName { get; set; }
        public DateTime ScheduleDate { get; set; }
        public TimeSpan ScheduleTime { get; set; }
        public int EstDelayMinutes { get; set; }
        public string ScheduleProcedure { get; set; }

        public Patient Patient { get; set; }
    }
}
