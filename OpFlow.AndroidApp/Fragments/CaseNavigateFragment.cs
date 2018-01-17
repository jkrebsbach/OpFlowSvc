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

namespace OpFlow.AndroidApp.Fragments
{
    public class CaseNavigateFragment : OpFlowFragmentBase
    {
        private string _circulatorName = "Roper, CRNA";

        private Patient _patient;
        private Surgery _surgery;
        private Flow _flow;
        private Card _card;
        private Room _room;

        private TextView _txtPatientName;
        private TextView _txtStartTime;
        private TextView _txtRoom;
        private TextView _txtPatientInfo;
        private TextView _txtFlowStep;
        private TextView _txtProcedure;


        private TextView _txtSurgeonName;
        private TextView _txtCirculatorName;
        private TextView _txtAnesName;
        private TextView _txtScrubName;
        private TextView _txtRepName;


        private Button _btnDebrief;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            AppSettings.CurrentScreen = AppSettings.FragmentEnum.CaseNavigate;
            base.OnCreateView(inflater, container, savedInstanceState);

            // Make sure we aren't disposing app
            if (container == null)
                return null;

            var rootView = inflater.Inflate(Resource.Layout.CaseNavigate, container, false);

            var pnlPatient = rootView.FindViewById<LinearLayout>(Resource.Id.pnlPatient);
            var pnlCard = rootView.FindViewById<LinearLayout>(Resource.Id.pnlCard);
            var pnlFlow = rootView.FindViewById<LinearLayout>(Resource.Id.pnlFlow);
            var pnlCommunicator = rootView.FindViewById<LinearLayout>(Resource.Id.pnlCommunicator);
            var btnDebrief = rootView.FindViewById<Button>(Resource.Id.btnDebrief);

            pnlPatient.Click += delegate
            {
                Listener.SendMessage(AppSettings.CurrentScreen, AppSettings.FragmentEnum.Patient);
            };
            pnlCard.Click += delegate
            {
                Listener.SendMessage(AppSettings.CurrentScreen, AppSettings.FragmentEnum.Patient);
            };
            pnlFlow.Click += delegate
            {
                Listener.SendMessage(AppSettings.CurrentScreen, AppSettings.FragmentEnum.Patient);
            };
            pnlCommunicator.Click += delegate
            {
                Listener.SendMessage(AppSettings.CurrentScreen, AppSettings.FragmentEnum.Communicator);
            };

            _txtPatientName = rootView.FindViewById<TextView>(Resource.Id.txtPatientName);
            _txtStartTime = rootView.FindViewById<TextView>(Resource.Id.txtStartTime);
            _txtRoom = rootView.FindViewById<TextView>(Resource.Id.txtRoom);
            _txtPatientInfo = rootView.FindViewById<TextView>(Resource.Id.txtPatientInfo);
            _txtFlowStep = rootView.FindViewById<TextView>(Resource.Id.txtFlowStep);
            _txtProcedure = rootView.FindViewById<TextView>(Resource.Id.txtProcedure);

            _txtSurgeonName = rootView.FindViewById<TextView>(Resource.Id.txtSurgeonName);
            _txtCirculatorName = rootView.FindViewById<TextView>(Resource.Id.txtCirculatorName);
            _txtAnesName = rootView.FindViewById<TextView>(Resource.Id.txtAnesName);
            _txtScrubName = rootView.FindViewById<TextView>(Resource.Id.txtScrubName);
            _txtRepName = rootView.FindViewById<TextView>(Resource.Id.txtRepName);

            return rootView;
        }

        public override async void OnResume()
        {
            base.OnResume();

            try
            {
                if (_patient == null || _surgery == null)
                {
                    _surgery = await SurgeryUtil.GetSurgery(
                        AppSettings.CurrentSurgery ?? -1, 
                        AppSettings.CurrentUser.ProviderID,
                        AppSettings.CurrentUser.LocationID);

                    if (_surgery == null)
                        return;

                    _patient = await PatientUtil.GetPatient(_surgery.PatientID);

                    if (_patient == null)
                        return;

                    var users = await SurgeryUtil.GetSurgeryUsers(
                        _surgery.CaseID,
                        _surgery.ProviderID,
                        _surgery.LocationID);

                    _flow = await FlowUtil.GetFlow(_surgery.FlowID, _surgery.CardID);
                    _card = (await CardUtil.GetCardData(_surgery.CardID)).FirstOrDefault();

                    _room = await AppSettings.GetRoom(_surgery.LocationID, _surgery.RoomID);

                    _txtPatientName.Text = PatientNameText;
                    _txtStartTime.Text = _surgery.ScheduleTime.ToString(@"hh\:mm");
                    _txtRoom.Text = _room.RoomDescription;
                    _txtPatientInfo.Text = PatientInfoText;
                    _txtFlowStep.Text = FlowText;
                    _txtProcedure.Text = _surgery.ProcedureDescription;

                    _txtSurgeonName.Text = GetUserName(users, AppSettings.RoleEnum.Surgeon);
                    _txtCirculatorName.Text = GetUserName(users, AppSettings.RoleEnum.Circulator);
                    _txtScrubName.Text = GetUserName(users, AppSettings.RoleEnum.ScrubTech);
                    _txtAnesName.Text = GetUserName(users, AppSettings.RoleEnum.FrontDesk);
                    _txtRepName.Text = GetUserName(users, AppSettings.RoleEnum.FrontDesk);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string GetUserName(List<SurgeryUser> users, AppSettings.RoleEnum roleId)
        {
            var user = users.FirstOrDefault(u => u.RoleID == (int) roleId);
            
            return user == null ? string.Empty : string.Format("{0}, {1} {2}", user.LastName, user.FirstName, user.Title);
        }

        private string PatientNameText => string.Format("{0}, {1}", _patient.LastName, _patient.FirstName);

        private string PatientInfoText => string.Format("{0} {1}", _patient.BirthDate.CalculateAge(), _patient.Sex);
        

        private string FlowText => string.Format("{0} - Flow: {1}",
            _flow.Description,
            _surgery.FlowID);
        
    }
}