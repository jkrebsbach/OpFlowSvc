using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data.Administration
{
    public class ScheduleImport : IImportData
    {
        public string PatientID { get; set; }
        public string CaseID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; }
        public int BMI { get; set; }
        public string MedicalHistory { get; set; }
        public string RiskFactors { get; set; }
        public string Medications { get; set; }
        public string Allergies { get; set; }
        public string Notes { get; set; }
        public DateTime ScheduleDate { get; set; }
        public string ScheduleTime { get; set; }
        public string Location { get; set; }
        public string Room { get; set; }
        public string Procedure { get; set; }
        public string ProcedureCard { get; set; }
        public string Surgeon { get; set; }
        public string Circulator { get; set; }
        public string Anes { get; set; }
        public string Tech { get; set; }
    }
}
