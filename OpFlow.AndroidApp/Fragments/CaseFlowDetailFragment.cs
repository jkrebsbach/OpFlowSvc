using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpFlow.Mobile;

namespace OpFlow.AndroidApp.Fragments
{
    public class CaseFlowDetailFragment : CaseDetailFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            AppSettings.CurrentScreen = AppSettings.FragmentEnum.FlowDetail;
            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        protected override async Task<List<DetailItem>> GetDetailTokens()
        {
            ToggleConfirm(false, "Update Flow Instructions");

            var flowSteps = await FlowUtil.GetFlowInstructions(Surgery.FlowID);
            var caseDetailTokens = await CaseDetailCategory.GetCaseDetailTokens(Surgery, Patient);

            //var caseDetailTokens = flowSteps
            //    .OrderBy(fs => fs.StepID)
            //    .Select(value => new CaseDetailToken(value))
            //    .ToList();

            return caseDetailTokens;
        }

    }
}