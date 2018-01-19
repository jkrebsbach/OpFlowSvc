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
using OpFlow.AndroidApp.Adapters;
using OpFlow.AndroidApp.Fragments;
using OpFlow.Data;
using OpFlow.Mobile;

namespace OpFlow.AndroidApp.Fragments
{
    public class ScheduleFragment : WeekDayPickerFragment
    {
        private GridView _gvDailySchedule;
        private Switch _swtSurgeon;
        private Spinner _spnRoom;

        private List<Surgery> _schedule;
        private Dictionary<int, Patient> _schedulePatients;

        private ProgressDialog _progressDialog;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            AppSettings.CurrentScreen = AppSettings.FragmentEnum.Schedule;
            base.OnCreateView(inflater, container, savedInstanceState);
            
            // Make sure we aren't disposing app
            if (container == null)
                return null;

            var rootView = inflater.Inflate(Resource.Layout.Schedule, container, false);

            _gvDailySchedule = rootView.FindViewById<GridView>(Resource.Id.gvDailySchedule);
            _swtSurgeon = rootView.FindViewById<Switch>(Resource.Id.swtSurgeon);
            _spnRoom = rootView.FindViewById<Spinner>(Resource.Id.spnRoom);
            _spnRoom.ItemSelected += async delegate
            {
                await LoadSchedule();
            };

            _gvDailySchedule.ItemClick += async delegate(object sender, AdapterView.ItemClickEventArgs eventArgs)
            {
                await CardItemClicked(sender, eventArgs);
            };

            _swtSurgeon.CheckedChange += async delegate(object sender, CompoundButton.CheckedChangeEventArgs e)
            {
                _spnRoom.Visibility = (e.IsChecked ? ViewStates.Gone : ViewStates.Visible);

                await LoadSchedule();
            };

            _progressDialog = new ProgressDialog(Context);
            _progressDialog.SetMessage("Please wait...");


            InitializePicker(rootView);

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

        private async Task CardItemClicked(object sender, AdapterView.ItemClickEventArgs eventArgs)
        {
            if (_schedule == null)
                return;

            var surgery = _schedule[eventArgs.Position];

            _previousFilter = null;

            _progressDialog.SetTitle("Loading Surgery...");
            _progressDialog.Show();
            
            AppSettings.LoadSurgery(surgery.SurgeryID, surgery.PatientID);

            _progressDialog.Hide();

            Listener.SendMessage(AppSettings.FragmentEnum.Schedule, surgery);
        }

        protected override async Task SelectDate()
        {
            await LoadSchedule();
        }

        private async Task SetupScreen()
        {
            if (!AppSettings.UserAuthenticated)
                return;

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
            _progressDialog.SetTitle("Searching...");
            _progressDialog.Show();

            await ScheduleQuery();

            _gvDailySchedule.Adapter = new ScheduleGridAdapter(Activity, _schedule, _schedulePatients);

            _progressDialog.Hide();
        }

        private async Task ScheduleQuery()
        {
            if (_swtSurgeon.Checked)
            {
                if (_previousFilter != null && _previousFilter.SurgeonOnly)
                    return;

                _schedule = await SurgeryUtil.GetSurgeryUserSchedule(SelectedDate);
                _previousFilter = new SurgeryFilter() { SurgeonOnly = true };
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

            _schedulePatients = await SurgeryUtil.GetSurgeryPatients(_schedule);
        }

        private class SurgeryFilter
        {
            public bool SurgeonOnly;
            public int RoomId;
        }
    }
}