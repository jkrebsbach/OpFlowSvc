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
using OpFlow.Data;
using OpFlow.Mobile;

namespace OpFlow.Android.Fragments
{
    public class CheckInFragment : OpFlowFragmentBase
    {

        private LinearLayout _pnlCaseDetail;
        private TextView _txtSurgeon;
        private TextView _txtCase;
        private TextView _txtCard;


        private Switch _swtCheckIn;
        private Spinner _spnAssignment;
        private GridView _gvSurgeryUsers;

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

            _swtCheckIn = rootView.FindViewById<Switch>(Resource.Id.swtCheckIn);
            _spnAssignment = rootView.FindViewById<Spinner>(Resource.Id.spnAssignment);
            _gvSurgeryUsers = rootView.FindViewById<GridView>(Resource.Id.gvSurgeryUsers);

            // Hide case detail until search completes
            _pnlCaseDetail.Visibility = ViewStates.Gone;

            var btnSearch = rootView.FindViewById<Button>(Resource.Id.btnSearch);
            btnSearch.Click += async delegate
            {
                // Search event should yield a Case ID
                await LoadCase();
            };


            _swtCheckIn.CheckedChange += async delegate
            {
                await CheckInUser();
            };
            
            return rootView;
        }

        public override async void OnResume()
        {
            base.OnResume();

            try
            {
                // It is possible to open this activity with a case selected
                if (AppSettings.CurrentSurgery != null)
                {
                    await LoadCase();
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task CheckInUser()
        {
            var user = AppSettings.CurrentUser;
            var surgeryId = AppSettings.CurrentSurgery.SurgeryID;

            await UserUtil.CheckoutUser(user, surgeryId);
        }

        private async Task LoadCase()
        {
            var locationId = AppSettings.CurrentUser.LocationID;
            var providerId = AppSettings.CurrentUser.ProviderID;
            var surgeryId = AppSettings.CurrentSurgery.SurgeryID;

            var currentSurgery = await SurgeryUtil.GetSurgery(
                surgeryId, locationId, providerId);

            // Something could go wrong?...
            if (currentSurgery == null)
                return;

            var currentUsers = await SurgeryUtil.GetSurgeryUsers(currentSurgery.CaseID,
                providerId, locationId);

            var rooms = await RoomUtil.GetRooms(currentSurgery.LocationID);

            _pnlCaseDetail.Visibility = ViewStates.Visible;

            _txtSurgeon.Text = $"{currentSurgery.SurgeonLastName}, {currentSurgery.SurgeonFirstName}";
            _txtCase.Text = currentSurgery.ProcedureDescription;
            _txtCard.Text = currentSurgery.CardDescription;

            var roomAdapter = new SpinnerAdapter<Room>(rooms);
            
            _swtCheckIn.Text = AppSettings.CurrentUser.RoleID.ToString();

            var adapter = new ArrayAdapter<string>(
                Activity, global::Android.Resource.Layout.SimpleSpinnerItem, roomAdapter.DropDownValues);
            adapter.SetDropDownViewResource(global::Android.Resource.Layout.SimpleSpinnerItem);

            _spnAssignment.Adapter = adapter;


            _gvSurgeryUsers.Adapter = new CheckInOverviewAdapter(Context, currentUsers);
        }
    }
}