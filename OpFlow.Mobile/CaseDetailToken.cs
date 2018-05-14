using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public abstract class DetailItem
    { }

    public class CaseDetailCategory : DetailItem
    {
        public string CategoryTitle;
        public readonly int CategoryGroupId;
        public bool HiddenDetails;

        public readonly List<CaseDetailToken> Tokens;
        public readonly List<CaseImageToken> Images;

        public CaseDetailCategory(int groupId, string categoryTitle)
        {
            CategoryTitle = categoryTitle;
            CategoryGroupId = groupId;

            Tokens = new List<CaseDetailToken>();
            Images = new List<CaseImageToken>();
        }

        public CaseDetailCategory(int groupId, string categoryTitle, int detailId, string detailText)
            : this(groupId, categoryTitle)
        {
            Tokens.Add(new CaseDetailToken(detailId, detailText, null, this));
        }
        
        public static async Task<List<DetailItem>> GetCaseDetailTokens(
            Surgery surgery, Patient patient)
        {
            List<CaseDetailCategory> categories = null;

            switch (AppSettings.CurrentScreen)
            {
                case AppSettings.FragmentEnum.Debrief:
                    categories = await GetFlowDebrief(surgery.SurgeryID);
                    break;
                case AppSettings.FragmentEnum.Patient:
                    categories = GetPatientDetails(patient);
                    break;
                case AppSettings.FragmentEnum.CardDetail:
                    categories = await GetCardItemDetails(surgery.CardID ?? 0);
                    break;
                case AppSettings.FragmentEnum.FlowDetail:
                    categories = await GetFlowDetails(surgery.FlowID ?? 0, surgery.SurgeryID);
                    break;
                default:
                    categories = GetPatientDetails(patient);
                    break;
            }

            var result = new List<DetailItem>();
            foreach (var category in categories)
            {
                result.Add(category);

                result.AddRange(category.Tokens);
                result.AddRange(category.Images ?? new List<CaseImageToken>());
            }

            return result;
        }

        private static async Task<List<CaseDetailCategory>> GetFlowDetails(int flowId, int surgeryId)
        {
            var result = new List<CaseDetailCategory>();
            var flowInstructions = await FlowUtil.GetFlowInstructions(flowId, surgeryId);

			foreach (var flowStep in flowInstructions)
            {
                foreach (var flowRole in flowStep.FlowRoleInstructions)
				{
					foreach (var flowInstruction in flowRole.FlowInstructions)
					{
						var category = result.FirstOrDefault(c => c.CategoryGroupId == flowStep.StepID);
					    if (category == null)
					    {
					        category = new CaseDetailCategory(flowStep.StepID, flowStep.StepName);
					        result.Add(category);
					    }

						category.Tokens.Add(new CaseDetailToken(0, 
						      flowInstruction.StepInstruction, flowInstruction.RoleDescription, category));
					}
				}
            }

            var flowImages = await FlowUtil.GetFlowImages(flowId, surgeryId);

            foreach (var flowImage in flowImages.FlowImages.OrderBy(fi => fi.FlowStep))
            {
                var category = result.FirstOrDefault(c => c.CategoryGroupId == flowImage.FlowStepID);
                if (category == null)
                {
                    category = new CaseDetailCategory(flowImage.FlowStepID, flowImage.FlowStep);
                    result.Add(category);
                }

                category.Images.Add(new CaseImageToken(flowId.ToString(),
                    flowImage.FlowImageID.ToString(), CaseImageToken.ImageTypeEnum.FlowImage, category));
            }
            foreach (var surgeryImage in flowImages.SurgeryImages.OrderBy(si => si.FlowStep))
            {
                var category = result.FirstOrDefault(c => c.CategoryGroupId == surgeryImage.FlowStepID);
                if (category == null)
                {
                    category = new CaseDetailCategory(surgeryImage.FlowStepID, surgeryImage.FlowStep);
                    result.Add(category);
                }

                category.Images.Add(new CaseImageToken(surgeryId.ToString(),
                    surgeryImage.SurgeryImageID.ToString(), CaseImageToken.ImageTypeEnum.SurgeryImage, category));
            }


            return result;
        }

        private static async Task<List<CaseDetailCategory>> GetFlowDebrief(int surgeryId)
        {
            var flowSteps = await FlowUtil.GetFlowSurgeryTimings(surgeryId);

            var caseDetailCategories = flowSteps
                .OrderBy(fs => fs.StepID)
                .GroupBy(x => new { x.StepID, x.StepDescription })
                .Select(value => new CaseDetailCategory(value.Key.StepID, value.Key.StepDescription))
                .ToList();

            foreach (var category in caseDetailCategories)
            {
                category.Tokens.Add(new CaseDetailToken(0, string.Empty, category.CategoryTitle, category));
            }
            return caseDetailCategories;
        }

        private static List<CaseDetailCategory> GetPatientDetails(Patient patient)
        {
            var categories = new List<CaseDetailCategory>();
            var demoValues = Enum.GetValues(typeof(PatientDemo.DemoTypeEnum))
                .Cast<PatientDemo.DemoTypeEnum>();

            foreach (var patientDemoDetail in demoValues)
            {
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

                categories.Add(new CaseDetailCategory((int)patientDemoDetail, patientDemoDetail.ToString(),
                    (int)patientDemoDetail, demoDetail.DemoDescription));
            }

            return categories;
        }
        
        private static async Task<List<CaseDetailCategory>> GetCardItemDetails(int cardId)
        {
            var items = await CardUtil.GetCardItems(cardId);


            var itemTypes = items
                .OrderBy(fs => fs.ItemType)
                .GroupBy(x => new { TypeID = 0, x.ItemType })
                .Select(value => new CaseDetailCategory(value.Key.TypeID, value.Key.ItemType))
                .ToList();
            
            foreach (var item in items)
            {
                var itemType = itemTypes.First(t => t.CategoryTitle == item.ItemType);

                var token = itemType.Tokens.FirstOrDefault();
                if (token == null)
                {
                    token = new CaseDetailToken(item.ItemID, item.ItemDescription, null, itemType);
                    itemType.Tokens.Add(token);
                }
                else
                {
                    token.DetailText += "\n" + item.ItemDescription;
                }
            }

            return itemTypes;
        }
    }

    public class CaseDetailToken : DetailItem
    {
        public string DetailText;
        public string DetailHeader;
        public int DetailId;
        public readonly CaseDetailCategory Category;

        public CaseDetailToken(int detailId, string detailText, string detailHeader, CaseDetailCategory category)
        {
            DetailId = detailId;
            DetailText = detailText;
            DetailHeader = detailHeader;

            Category = category;
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
    }

    public class CaseImageToken : DetailItem
    {
        public string FolderID;
        public string DetailID;
        public ImageTypeEnum ImageType;

        public readonly CaseDetailCategory Category;

        public CaseImageToken(string folderId, string detailId, ImageTypeEnum imageType, CaseDetailCategory category)
        {
            FolderID = folderId;
            DetailID = detailId;
            ImageType = imageType;

            Category = category;
        }

        public enum ImageTypeEnum
        {
            FlowImage,
            SurgeryImage
        }
    }
}
