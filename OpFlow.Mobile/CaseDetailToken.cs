using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public class CaseDetailToken
    {
        public string CategoryTitle;
        public int CategoryGroupId;
        public string DetailText;
        public int DetailId;
        
        public CaseDetailToken(Patient patient, PatientDemo.DemoTypeEnum patientDemoDetail)
        {
            CategoryTitle = patientDemoDetail.ToString();
            PatientDemo demoDetail = null;

            switch (patientDemoDetail)
            {
                case PatientDemo.DemoTypeEnum.MedicalHistory:
                    demoDetail = patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.MedicalHistory);
                    break;
                case PatientDemo.DemoTypeEnum.RiskFactors:
                    demoDetail = patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.RiskFactors);
                    break;
                case PatientDemo.DemoTypeEnum.Medications:
                    demoDetail = patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.Medications);
                    break;
                case PatientDemo.DemoTypeEnum.Allergies:
                    demoDetail = patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.Allergies);
                    break;
                case PatientDemo.DemoTypeEnum.LabResults:
                    demoDetail = patient.DemoData.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.LabResults);
                    break;
                case PatientDemo.DemoTypeEnum.PastProcedureResult:
                    demoDetail = patient.DemoData.FirstOrDefault(pd =>
                            pd.DemoType == PatientDemo.DemoTypeEnum.PastProcedureResult);
                    break;
            }

            CategoryGroupId = (int) patientDemoDetail;
            DetailId = CategoryGroupId;
            DetailText = demoDetail?.DemoDescription ?? "";
        }

        public CaseDetailToken(FlowStep flowStep)
        {
            CategoryTitle = flowStep.StepDescription;
            DetailText = flowStep.StepInstruction;

            CategoryGroupId = flowStep.StepID;
            DetailId = flowStep.StepID;
        }
    }
}
