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
        private Switch _swtSurgeon;
        private Spinner _spnRoom;

        private List<Surgery> _schedule;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            AppSettings.CurrentScreen = AppSettings.FragmentEnum.Schedule;
            base.OnCreateView(inflater, container, savedInstanceState);
            
            // Make sure we aren't disposing app
            if (container == null)
                return null;

            var rootView = inflater.Inflate(Resource.Layout.Schedule, container, false);

            _btnSchedule = rootView.FindViewById<Button>(Resource.Id.btnScheduleDate);
            _gvDailySchedule = rootView.FindViewById<GridView>(Resource.Id.gvDailySchedule);
            _swtSurgeon = rootView.FindViewById<Switch>(Resource.Id.swtSurgeon);
            _spnRoom = rootView.FindViewById<Spinner>(Resource.Id.spnRoom);
            _spnRoom.ItemSelected += async delegate
            {
                await LoadSchedule();
            };

            _selectedDate = DateTime.Today;

            _btnSchedule.Click += btnSchedule_OnClick;

            _gvDailySchedule.ItemClick += CardItemClicked;

            _swtSurgeon.CheckedChange += async delegate(object sender, CompoundButton.CheckedChangeEventArgs e)
            {
                _spnRoom.Visibility = (e.IsChecked ? ViewStates.Gone : ViewStates.Visible);

                await LoadSchedule();
            };

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

            _previousFilter = null;
            Listener.SendMessage(AppSettings.FragmentEnum.Schedule, schedule);
        }

        void btnSchedule_OnClick(object sender, EventArgs eventArgs)
        {
            var frag = DatePickerFragment.NewInstance(_selectedDate, async delegate (DateTime time)
            {
                _selectedDate = time;
                _btnSchedule.Text = _selectedDate.ToString("M/d/yyyy");

                await LoadSchedule();
            });
            frag.Show(FragmentManager, DatePickerFragment.TAG);
        }

        private async Task SetupScreen()
        {
            if (!AppSettings.UserAuthenticated)
                return;

            _btnSchedule.Text = _selectedDate.ToString("M/d/yyyy");
            
            var rooms = await AppSettings.RoomList(AppSettings.CurrentUser.LocationID);

            var roomAdapter = new SpinnerAdapter<Room>(rooms);

            var adapter = new ArrayAdapter<string>(
                Activity, global::Android.Resource.Layout.SimpleSpinnerItem, roomAdapter.DropDownValues);
            adapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerItem);

            _spnRoom.Adapter = adapter;

            _spnRoom.Visibility = (_swtSurgeon.Checked ? ViewStates.Gone : ViewStates.Visible);

            await LoadSchedule();
        }

        // Use this to avoid dupe queries from various controls
        private SurgeryFilter _previousFilter;

        private async Task LoadSchedule()
        {
            if (_swtSurgeon.Checked)
            {
                if (_previousFilter != null && _previousFilter.SurgeonOnly)
                    return;

                _schedule = await SurgeryUtil.GetSurgeryUserSchedule(_selectedDate);
                _previousFilter = new SurgeryFilter() {SurgeonOnly = true};
            }
            else
            {
                var selectedRoom = _spnRoom.SelectedItem?.ToString() ?? "";
                var rooms = await AppSettings.RoomList(AppSettings.CurrentUser.LocationID);
                var roomId = rooms.FirstOrDefault(r => r.RoomDescription == selectedRoom)?.RoomID ?? 0;

                if ((roomId == 0) ||
                    (_previousFilter != null && _previousFilter.RoomId == roomId))
                    return;

                _schedule = await SurgeryUtil.GetSurgeryRoomSchedule(roomId);
                _previousFilter = new SurgeryFilter() { RoomId = roomId };
            }

            var schedulePatients = await SurgeryUtil.GetSurgeryPatients(_schedule);

            _gvDailySchedule.Adapter = new Adapters.ScheduleGridAdapter(Activity, _schedule, schedulePatients);

        }

        private class SurgeryFilter
        {
            public bool SurgeonOnly;
            public int RoomId;
        }
    }
}