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
using OpFlow.Mobile;

namespace OpFlow.Android.Activities
{
    [Activity(Label = "ScheduleActivity")]
    public class ScheduleActivity : Activity
    {
        private DateTime _selectedDate;
        private Button _btnSchedule;
        private GridView _gvDailySchedule;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Schedule);

            _btnSchedule = FindViewById<Button>(Resource.Id.btnScheduleDate);
            _gvDailySchedule = FindViewById<GridView>(Resource.Id.gvDailySchedule);

            _selectedDate = DateTime.Today;

            _btnSchedule.Click += btnSchedule_OnClick;


            await SetupScreen();
        }

        void btnSchedule_OnClick(object sender, EventArgs eventArgs)
        {
            DatePickerFragment frag = DatePickerFragment.NewInstance(_selectedDate, delegate (DateTime time)
            {
                _selectedDate = time;
                SetupScreen();
            });
            frag.Show(FragmentManager, DatePickerFragment.TAG);
        }

        private async Task SetupScreen()
        {
            _btnSchedule.Text = _selectedDate.ToString("M/d/yyyy");

            var schedule = await WebUtility.GetSchedules(_selectedDate);
            _gvDailySchedule.Adapter = new Adapters.ScheduleGridAdapter(this, schedule);
        }
    }
}