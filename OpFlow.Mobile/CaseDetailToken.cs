using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public class CaseDetailToken
    {
        public CaseDetailEnum CaseDetail;
        public string DetailText;
        
        public enum CaseDetailEnum
        {
            MedicalHistory,
            RiskFactors,
            Medications,
            Allergies,
            LabResults,
            PastProcedureResults,
            ScheduledProcedures
        }

        public CaseDetailToken(Patient patient, CaseDetailEnum caseDetail)
        {
            CaseDetail = caseDetail;
            PatientDemo demoDetail = null;

            switch (caseDetail)
            {
                case CaseDetailEnum.MedicalHistory:
                    demoDetail = patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.MedicalHistory);
                    break;
                case CaseDetailEnum.RiskFactors:
                    demoDetail = patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.RiskFactors);
                    break;
                case CaseDetailEnum.Medications:
                    demoDetail = patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.Medications);
                    break;
                case CaseDetailEnum.Allergies:
                    demoDetail = patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.Allergies);
                    break;
                case CaseDetailEnum.LabResults:
                    demoDetail = patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.LabResults);
                    break;
                case CaseDetailEnum.PastProcedureResults:
                    demoDetail = patient.DemoData.FirstOrDefault(pd =>
                            pd.DemoType == PatientDemo.DemoTypeEnum.PastProcedureResult);
                    break;
            }

            DetailText = demoDetail?.DemoDescription ?? "";
        }
    }
}
