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
            var pnlDashboard = rootView.FindViewById<LinearLayout>(Resource.Id.pnlDashboard);
            var btnDebrief = rootView.FindViewById<Button>(Resource.Id.btnDebrief);

            pnlPatient.Click += delegate
            {
                Listener.SendMessage(AppSettings.CurrentScreen, AppSettings.FragmentEnum.Patient);
            };
            pnlCard.Click += delegate
            {
                Listener.SendMessage(AppSettings.CurrentScreen, AppSettings.FragmentEnum.CardDetail);
            };
            pnlFlow.Click += delegate
            {
                Listener.SendMessage(AppSettings.CurrentScreen, AppSettings.FragmentEnum.FlowDetail);
            };
            pnlDashboard.Click += delegate
            {
                Listener.SendMessage(AppSettings.CurrentScreen, AppSettings.FragmentEnum.Dashboard);
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

                    _flow = await FlowUtil.GetFlow(_surgery.FlowID, _surgery.CardID);
                    _card = (await CardUtil.GetCardData(_surgery.CardID)).FirstOrDefault();

                    _patient = await PatientUtil.GetPatient(_surgery.PatientID);

                    var users = await SurgeryUtil.GetSurgeryUsers(
                        _surgery.CaseID);

                    _txtSurgeonName.Text = GetUserName(users, RoleEnum.Surgeon);
                    _txtCirculatorName.Text = GetUserName(users, RoleEnum.Circulator);
                    _txtScrubName.Text = GetUserName(users, RoleEnum.ScrubTech);
                    _txtAnesName.Text = GetUserName(users, RoleEnum.FrontDesk);
                    _txtRepName.Text = GetUserName(users, RoleEnum.FrontDesk);

                    _room = await AppSettings.GetRoom(_surgery.LocationID, _surgery.RoomID);

                    _txtPatientName.Text = PatientNameText;
                    _txtStartTime.Text = _surgery.ScheduleTime.ToString(@"hh\:mm");
                    _txtRoom.Text = _room?.RoomDescription;
                    _txtPatientInfo.Text = PatientInfoText;
                    _txtFlowStep.Text = FlowText;
                    _txtProcedure.Text = _surgery.ProcedureDescription;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string GetUserName(List<SurgeryUser> users, RoleEnum roleId)
        {
            var user = users.FirstOrDefault(u => u.RoleID == (int) roleId);
            
            return user == null ? string.Empty : string.Format("{0}, {1} {2}", user.LastName, user.FirstName, user.UserTitle);
        }

        private string PatientNameText => string.Format("{0}, {1}", _patient?.LastName, _patient?.FirstName);

        private string PatientInfoText => string.Format("{0} {1}", _patient?.BirthDate.CalculateAge(), _patient?.Gender);
        

        private string FlowText => string.Format("{0} - Flow: {1}",
            _flow.FlowDescription,
            _surgery.FlowID);
        
    }
}