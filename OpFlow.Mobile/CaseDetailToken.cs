using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.Mobile
{
    public class CaseDetailToken
    {
        public CaseDetailEnum CaseDetail;
        
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

        public CaseDetailToken(CaseDetailEnum caseDetail)
        {
            CaseDetail = caseDetail;
        }
    }
}
