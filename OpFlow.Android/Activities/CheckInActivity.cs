using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.CheckIn);

            var pnlCaseDetail = FindViewById<LinearLayout>(Resource.Id.pnlCaseDetail);
            var txtSurgeon = FindViewById<TextView>(Resource.Id.txtSurgeon);
            var txtCase = FindViewById<TextView>(Resource.Id.txtCase);
            var txtCard = FindViewById<TextView>(Resource.Id.txtCard);

            // Hide case detail until search completes
            pnlCaseDetail.Visibility = ViewStates.Gone;

            var btnSearch = FindViewById<Button>(Resource.Id.btnSearch);
            btnSearch.Click += async delegate
            {
                try
                {
                    var currentCase = await CaseUtil.GetCase(1);

                    pnlCaseDetail.Visibility = ViewStates.Visible;

                    txtSurgeon.Text = string.Format("{0}, {1}", currentCase.ProviderLastName,
                        currentCase.ProviderFirstName);
                    txtCase.Text = currentCase.Card.ProcedureName;
                    txtCard.Text = currentCase.Card.CardName;
                }
                catch (Exception ex)
                {
                    
                }
            };
        }
    }
}