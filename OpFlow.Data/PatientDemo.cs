using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Data
{
    public class PatientDemo
    {
        public int PatientDemoID { get; set; }
        public  int PatientID { get; set; }
        public DataTypeEnum DataType { get; set; }
        public string Descrpition { get; set; }
        public string Notes { get; set; }

        public enum DataTypeEnum
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
