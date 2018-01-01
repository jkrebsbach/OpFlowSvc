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
using OpFlow.Android.Adapters;
using OpFlow.Android.Fragments;
using OpFlow.Data;
using OpFlow.Mobile;

namespace OpFlow.Android.Activities
{
    [Activity(Label = "ScheduleActivity")]
    public class ScheduleActivity : OpFlowActivityBase
    {
        private DateTime _selectedDate;
        private Button _btnSchedule;
        private GridView _gvDailySchedule;

        private List<Surgery> _schedule;

        protected override int GetLayoutResourceId()
        {
            return Resource.Layout.Schedule;
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            _btnSchedule = FindViewById<Button>(Resource.Id.btnScheduleDate);
            _gvDailySchedule = FindViewById<GridView>(Resource.Id.gvDailySchedule);

            _selectedDate = DateTime.Today;

            _btnSchedule.Click += btnSchedule_OnClick;

            _gvDailySchedule.ItemClick += CardItemClicked;

            await SetupScreen();
        }

        private void CardItemClicked(object sender, AdapterView.ItemClickEventArgs eventArgs)
        {
            if (_schedule == null)
                return;

            var schedule = _schedule[eventArgs.Position];
            
            var checkInActivity = new Intent(this, typeof(CheckInActivity));
            checkInActivity.PutExtra(AndroidApp.SURGERY_BUNDLE, schedule.SurgeryID.ToString());
            StartActivity(checkInActivity);
        }

        void btnSchedule_OnClick(object sender, EventArgs eventArgs)
        {
            var frag = DatePickerFragment.NewInstance(_selectedDate, async delegate (DateTime time)
            {
                _selectedDate = time;
                await SetupScreen();
            });
            frag.Show(FragmentManager, DatePickerFragment.TAG);
        }

        private async Task SetupScreen()
        {
            _btnSchedule.Text = _selectedDate.ToString("M/d/yyyy");

            _schedule = await SurgeryUtil.GetSurgerySchedule(_selectedDate);
            _gvDailySchedule.Adapter = new Adapters.ScheduleGridAdapter(this, _schedule);
        }
    }
}