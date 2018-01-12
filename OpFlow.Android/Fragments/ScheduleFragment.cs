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

namespace OpFlow.Android.Fragments
{
    public class ScheduleFragment : OpFlowFragmentBase
    {
        private DateTime _selectedDate;
        private Button _btnSchedule;
        private GridView _gvDailySchedule;

        private List<Surgery> _schedule;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);

            // Make sure we aren't disposing app
            if (container == null)
                return null;

            var rootView = inflater.Inflate(Resource.Layout.Schedule, container, false);

            _btnSchedule = rootView.FindViewById<Button>(Resource.Id.btnScheduleDate);
            _gvDailySchedule = rootView.FindViewById<GridView>(Resource.Id.gvDailySchedule);

            _selectedDate = DateTime.Today;

            _btnSchedule.Click += btnSchedule_OnClick;

            _gvDailySchedule.ItemClick += CardItemClicked;

            return rootView;
        }

        public override async void OnResume()
        {
            base.OnResume();

            try
            {
                await SetupScreen();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private void CardItemClicked(object sender, AdapterView.ItemClickEventArgs eventArgs)
        {
            if (_schedule == null)
                return;

            var schedule = _schedule[eventArgs.Position];

            Listener.SendMessage(FragmentEnum.Schedule, schedule);
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
            if (!AppSettings.UserAuthenticated)
                return;

            _btnSchedule.Text = _selectedDate.ToString("M/d/yyyy");

            _schedule = await SurgeryUtil.GetSurgerySchedule(_selectedDate);
            var schedulePatients = await SurgeryUtil.GetSurgeryPatients(_schedule);

            _gvDailySchedule.Adapter = new Adapters.ScheduleGridAdapter(Activity, _schedule, schedulePatients);
        }
    }
}