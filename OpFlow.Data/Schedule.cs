using System;

namespace OpFlow.Data
{
    public class Schedule
    {
        public int SurgeryID { get; set; }
        public int PatientID { get; set; }
        public string PatientFirstName { get; set; }
        public string PatientLastName { get; set; }
        public DateTime PatientBirthDate { get; set; }
        public string PatientSex { get; set; }
        public decimal PatientBMI { get; set; }
        public DateTime ScheduleTime { get; set; }
        public string ScheduleProcedure { get; set; }
    }
}
