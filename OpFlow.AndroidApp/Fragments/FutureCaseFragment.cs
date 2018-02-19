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
using OpFlow.Data;
using OpFlow.Mobile;

namespace OpFlow.AndroidApp.Fragments
{
    public class FutureCaseFragment : OpFlowFragmentBase
    {
        private List<Surgery> _schedule;
        private Dictionary<int, Patient> _surgeryPatients;
        private ListView _lvFutureCases;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            AppSettings.CurrentScreen = AppSettings.FragmentEnum.CardList;
            base.OnCreateView(inflater, container, savedInstanceState);
            
            // Make sure we aren't disposing app
            if (container == null)
                return null;

            var rootView = inflater.Inflate(Resource.Layout.FutureCase, container, false);

            _lvFutureCases = rootView.FindViewById<ListView>(Resource.Id.lvFutureCases);
            _lvFutureCases.ItemClick += async delegate(object sender, AdapterView.ItemClickEventArgs eventArgs)
            {
                await FutureCaseClicked(sender, eventArgs);
            };

            return rootView;
        }

        public override async void OnResume()
        {
            base.OnResume();

            await SetupScreen();
        }

        private async Task SetupScreen()
        {
            _schedule = await SurgeryUtil.GetSurgeryUserSchedule(DateTime.Now);
            _surgeryPatients = await SurgeryUtil.GetSurgeryPatients(_schedule);

            _lvFutureCases.Adapter = new Adapters.FutureCaseListAdapter(Activity, _schedule, _surgeryPatients);
        }

        private async Task FutureCaseClicked(object sender, AdapterView.ItemClickEventArgs eventArgs)
        {
            if (_schedule == null)
                return;

            var schedule = _schedule[eventArgs.Position];

            AppSettings.LoadSurgery(schedule.SurgeryID, schedule.PatientID);

            Listener.SendMessage(AppSettings.FragmentEnum.CardList, schedule);
        }
    }
}