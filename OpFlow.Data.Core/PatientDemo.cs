using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class PatientDemo
    {
        public  int PatientID { get; set; }
        public DemoTypeEnum DemoType { get; set; }
        public string DemoDescription { get; set; }
        public string DemoNotes { get; set; }

        public enum DemoTypeEnum
        {
            MedicalHistory = 1,
            RiskFactors = 2,
            Medications = 3,
            Allergies = 4,
            LabResults = 5,
            PastProcedureResult = 6
        }
    }
}
