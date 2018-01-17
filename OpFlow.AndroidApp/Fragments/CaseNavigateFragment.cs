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

        private TextView _txtCheckIn;
        private TextView _txtCase;
        private TextView _txtCard;
        private TextView _txtFlow;
        private TextView _txtRoom;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            AppSettings.CurrentScreen = AppSettings.FragmentEnum.CaseNavigate;
            base.OnCreateView(inflater, container, savedInstanceState);

            // Make sure we aren't disposing app
            if (container == null)
                return null;

            var rootView = inflater.Inflate(Resource.Layout.CaseNavigate, container, false);

            var pnlCheckIn = rootView.FindViewById<LinearLayout>(Resource.Id.pnlCheckIn);
            var pnlCase = rootView.FindViewById<LinearLayout>(Resource.Id.pnlCase);
            var pnlCard = rootView.FindViewById<LinearLayout>(Resource.Id.pnlCard);
            var pnlFlow = rootView.FindViewById<LinearLayout>(Resource.Id.pnlFlow);
            var pnlRoom = rootView.FindViewById<LinearLayout>(Resource.Id.pnlRoom);

            pnlCheckIn.Click += delegate
            {
                Listener.SendMessage(AppSettings.CurrentScreen, AppSettings.FragmentEnum.CheckIn);
            };
            pnlCase.Click += delegate
            {
                Listener.SendMessage(AppSettings.CurrentScreen, AppSettings.FragmentEnum.CaseDetail);
            };
            pnlCard.Click += delegate
            {
                Listener.SendMessage(AppSettings.CurrentScreen, AppSettings.FragmentEnum.CaseDetail);
            };
            pnlFlow.Click += delegate
            {
                Listener.SendMessage(AppSettings.CurrentScreen, AppSettings.FragmentEnum.CaseDetail);
            };
            pnlRoom.Click += delegate
            {
                Listener.SendMessage(AppSettings.CurrentScreen, AppSettings.FragmentEnum.CaseDetail);
            };


            _txtCheckIn = rootView.FindViewById<TextView>(Resource.Id.txtCheckIn);
            _txtCase = rootView.FindViewById<TextView>(Resource.Id.txtCase);
            _txtCard = rootView.FindViewById<TextView>(Resource.Id.txtCard);
            _txtFlow = rootView.FindViewById<TextView>(Resource.Id.txtFlow);
            _txtRoom = rootView.FindViewById<TextView>(Resource.Id.txtRoom);

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
                    _patient = await PatientUtil.GetPatient(_surgery.PatientID);

                    _flow = await FlowUtil.GetFlow(_surgery.FlowID, _surgery.CardID);
                    _card = (await CardUtil.GetCardData(_surgery.CardID)).FirstOrDefault();

                    _room = await AppSettings.GetRoom(_surgery.LocationID, _surgery.RoomID);

                    _txtCheckIn.Text = CheckInText;
                    _txtCase.Text = CaseText;
                    _txtCard.Text = CardText;
                    _txtFlow.Text = FlowText;
                    _txtRoom.Text = RoomText;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private string CheckInText => string.Format("{0}\nSurgeon: {1}\nCirc: {2}",
            _patient.LastName,
            _surgery.SurgeonLastName,
            _circulatorName);

        private string CaseText => "Surgeon notes about the case";

        private string CardText => string.Format("{0} - Card: {1}", 
            _card.CardDescription,
            _surgery.CardID);

        private string FlowText => string.Format("{0} - Flow: {1}",
            _flow.Description,
            _surgery.FlowID);

        private string RoomText => string.Format("Room ID: {0}", _room.RoomDescription);
    }
}