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
    public class CaseDebriefFragment : CaseDetailFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            AppSettings.CurrentScreen = AppSettings.FragmentEnum.Debrief;
            return base.OnCreateView(inflater, container, savedInstanceState);
        }

        protected override async Task<List<DetailItem>> GetDetailTokens()
        {
            ToggleConfirm(false, "Save Feedback");

            return await CaseDetailCategory.GetCaseDetailTokens(Surgery, Patient);
        }

        protected override void ButtonClicked(object sender, EventArgs e)
        {
            // Commit debrief message

            Listener.SendMessage(AppSettings.CurrentScreen, AppSettings.FragmentEnum.CaseNavigate);
        }
    }
}