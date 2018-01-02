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

namespace OpFlow.Android.Fragments
{
    public class CheckInFragment : OpFlowFragmentBase
    {

        private LinearLayout _pnlCaseDetail;
        private TextView _txtSurgeon;
        private TextView _txtCase;
        private TextView _txtCard;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            // Make sure we aren't disposing app
            if (container == null)
                return null;

            var rootView = inflater.Inflate(Resource.Layout.CheckIn, container, false);

            _pnlCaseDetail = rootView.FindViewById<LinearLayout>(Resource.Id.pnlCaseDetail);
            _txtSurgeon = rootView.FindViewById<TextView>(Resource.Id.txtSurgeon);
            _txtCase = rootView.FindViewById<TextView>(Resource.Id.txtCase);
            _txtCard = rootView.FindViewById<TextView>(Resource.Id.txtCard);

            // Hide case detail until search completes
            _pnlCaseDetail.Visibility = ViewStates.Gone;

            var btnSearch = rootView.FindViewById<Button>(Resource.Id.btnSearch);
            btnSearch.Click += async delegate
            {
                // Search event should yield a Case ID
                await LoadCase(1);
            };

            
            return rootView;
        }

        public override async void OnResume()
        {
            base.OnResume();

            try
            {
                // It is possible to open this activity with a case selected
                if (AndroidApp.SurgeryID > 0)
                {
                    await LoadCase(AndroidApp.SurgeryID);
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task LoadCase(int surgeryId)
        {
            var currentCase = await SurgeryUtil.GetSurgery(surgeryId);

            _pnlCaseDetail.Visibility = ViewStates.Visible;

            _txtSurgeon.Text = string.Format("{0}, {1}", currentCase.SurgeonLastName,
                currentCase.SurgeonFirstName);
            _txtCase.Text = currentCase.ProcedureDescription;
            _txtCard.Text = currentCase.CardDescription;
        }
    }
}