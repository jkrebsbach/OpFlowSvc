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
using OpFlow.Data;
using OpFlow.Mobile;

namespace OpFlow.Android.Activities
{
    [Activity(Label = "FutureCaseActivity")]
    public class FutureCaseActivity : Activity
    {
        private List<Schedule> _schedule;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.FutureCase);

            var lvFutureCases = FindViewById<ListView>(Resource.Id.lvFutureCases);

            _schedule = await ScheduleUtil.GetSchedule(DateTime.Now);
            lvFutureCases.Adapter = new Adapters.FutureCaseListAdapter(this, _schedule);

            lvFutureCases.ItemClick += FutureCaseClicked;
        }

        private void FutureCaseClicked(object sender, AdapterView.ItemClickEventArgs eventArgs)
        {
            if (_schedule == null)
                return;

            var schedule = _schedule[eventArgs.Position];

            var caseDetailActivity = new Intent(this, typeof(CaseDetailActivity));
            caseDetailActivity.PutExtra(CheckInActivity.CARD_BUNDLE, schedule.CardID.ToString());
            StartActivity(caseDetailActivity);
        }
    }
}