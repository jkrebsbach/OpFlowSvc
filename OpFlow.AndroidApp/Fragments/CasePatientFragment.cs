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
using OpFlow.Data;
using OpFlow.Mobile;

namespace OpFlow.AndroidApp.Fragments
{
    public class CasePatientFragment : CaseDetailFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            AppSettings.CurrentScreen = AppSettings.FragmentEnum.Patient;
            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        protected override async Task<List<CaseDetailToken>> GetDetailTokens()
        {
            ToggleConfirm(true, "PATIENT");

            return CaseDetailToken.GetFlowDetails(Patient);
        }
    }
}