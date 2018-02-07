using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public class CaseDetailToken
    {
        public string CategoryTitle;
        public int CategoryGroupId;
        public string DetailText;
        public int DetailId;

        public static List<CaseDetailToken> GetFlowDetails(Patient patient)
        {
            var caseDetailTokens = Enum.GetValues(typeof(PatientDemo.DemoTypeEnum))
                .Cast<PatientDemo.DemoTypeEnum>()
                .Select(value => new CaseDetailToken(patient, value))
                .ToList();

            return caseDetailTokens;
        }

        private CaseDetailToken(Patient patient, PatientDemo.DemoTypeEnum patientDemoDetail)
        {
            CategoryTitle = patientDemoDetail.ToString();
            PatientDemo demoDetail = null;

            switch (patientDemoDetail)
            {
                case PatientDemo.DemoTypeEnum.MedicalHistory:
                    demoDetail = patient?.DemoData?.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.MedicalHistory);
                    break;
                case PatientDemo.DemoTypeEnum.RiskFactors:
                    demoDetail = patient?.DemoData?.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.RiskFactors);
                    break;
                case PatientDemo.DemoTypeEnum.Medications:
                    demoDetail = patient?.DemoData?.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.Medications);
                    break;
                case PatientDemo.DemoTypeEnum.Allergies:
                    demoDetail = patient?.DemoData?.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.Allergies);
                    break;
                case PatientDemo.DemoTypeEnum.LabResults:
                    demoDetail = patient?.DemoData?.FirstOrDefault(pd => pd.DemoType == PatientDemo.DemoTypeEnum.LabResults);
                    break;
                case PatientDemo.DemoTypeEnum.PastProcedureResult:
                    demoDetail = patient?.DemoData?.FirstOrDefault(pd =>
                            pd.DemoType == PatientDemo.DemoTypeEnum.PastProcedureResult);
                    break;
            }

            if (demoDetail == null)
                demoDetail = new PatientDemo()
                {
                    DemoDescription = "NO DATA FOUND",
                };


            CategoryGroupId = (int) patientDemoDetail;
            DetailId = CategoryGroupId;
            DetailText = demoDetail?.DemoDescription ?? "";
        }

        public static async Task<List<CaseDetailToken>> GetCaseDetailTokens(
            Surgery surgery, Patient patient)
        {
            List<CaseDetailToken> tokens = null;

            switch (AppSettings.CurrentScreen)
            {
                case AppSettings.FragmentEnum.Debrief:
                    tokens = await GetFlowDetails(surgery.FlowID);
                    break;
                case AppSettings.FragmentEnum.Patient:
                    tokens = GetFlowDetails(patient);
                    break;
                case AppSettings.FragmentEnum.CardDetail:
                    tokens = await GetCardItemDetails(surgery.CardID);
                    break;
                case AppSettings.FragmentEnum.FlowDetail:
                    tokens = await GetFlowDetails(surgery.FlowID);
                    break;
                default:
                    tokens = GetFlowDetails(patient);
                    break;
            }

            return tokens;
        }

        public static bool AllowEdit()
        {
            switch (AppSettings.CurrentScreen)
            {
                case AppSettings.FragmentEnum.Debrief:
                    return true;
                case AppSettings.FragmentEnum.Patient:
                    return false;
                default:
                    return false;
            }
        }

        public static async Task<List<CaseDetailToken>> GetFlowDetails(int flowId)
        {
            var flowSteps = await FlowUtil.GetFlowTimings(flowId);

            var caseDetailTokens = flowSteps
                .OrderBy(fs => fs.StepID)
                .Select(value => new CaseDetailToken(value))
                .ToList();

            return caseDetailTokens;
        }

        private CaseDetailToken(FlowStep flowStep)
        {
            CategoryTitle = flowStep.StepDescription;
            DetailText = flowStep.StepInstruction;

            CategoryGroupId = flowStep.StepID;
            DetailId = flowStep.StepID;
        }

        private static async Task<List<CaseDetailToken>> GetCardItemDetails(int cardId)
        {
            var items = await CardUtil.GetCardItems(cardId);

            var caseDetailTokens = items
                .OrderBy(fs => fs.ItemID)
                .Select(value => new CaseDetailToken(value))
                .ToList();

            return caseDetailTokens;
        }

        private CaseDetailToken(CardItem cardItem)
        {
            CategoryTitle = cardItem.ItemType;
            DetailText = cardItem.ItemDescription;

            CategoryGroupId = cardItem.ItemID;
            DetailId = cardItem.ItemID;
        }
    }
}
