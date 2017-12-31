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

namespace OpFlow.Android.Activities
{
    [Activity(Label = "CheckInActivity")]
    public class CheckInActivity : Activity
    {

        private LinearLayout _pnlCaseDetail;
        private TextView _txtSurgeon;
        private TextView _txtCase;
        private TextView _txtCard;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.CheckIn);

            _pnlCaseDetail = FindViewById<LinearLayout>(Resource.Id.pnlCaseDetail);
            _txtSurgeon = FindViewById<TextView>(Resource.Id.txtSurgeon);
            _txtCase = FindViewById<TextView>(Resource.Id.txtCase);
            _txtCard = FindViewById<TextView>(Resource.Id.txtCard);

            // Hide case detail until search completes
            _pnlCaseDetail.Visibility = ViewStates.Gone;

            var btnSearch = FindViewById<Button>(Resource.Id.btnSearch);
            btnSearch.Click += async delegate
            {
                // Search event should yield a Case ID
                await LoadCase(1, 1, 1);
            };

            // It is possible to open this activity with a case selected
            var caseId = Intent.GetStringExtra(AndroidApp.CARD_BUNDLE);
            var locationId = Intent.GetStringExtra(AndroidApp.CARD_BUNDLE);
            var providerId = Intent.GetStringExtra(AndroidApp.CARD_BUNDLE);
            if (!string.IsNullOrEmpty(caseId))
            {
                await LoadCase(int.Parse(caseId), int.Parse(locationId), int.Parse(providerId));
            }
        }

        private async Task LoadCase(int caseId, int locationId, int providerId)
        {
            var currentCase = await CaseUtil.GetCase(caseId, locationId, providerId);

            _pnlCaseDetail.Visibility = ViewStates.Visible;

            _txtSurgeon.Text = string.Format("{0}, {1}", currentCase.SurgeonLastName,
                currentCase.SurgeonFirstName);
            _txtCase.Text = currentCase.ProcedureDescription;
            _txtCard.Text = currentCase.CardDescription;
        }
    }
}