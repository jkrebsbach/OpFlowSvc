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
using OpFlow.Mobile;

namespace OpFlow.AndroidApp.Fragments
{
    public class CaseNavigateFragment : OpFlowFragmentBase
    {
        private string _circulatorName = "Roper, CRNA";

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


            var txtCheckIn = rootView.FindViewById<TextView>(Resource.Id.txtCheckIn);
            var txtCase = rootView.FindViewById<TextView>(Resource.Id.txtCase);
            var txtCard = rootView.FindViewById<TextView>(Resource.Id.txtCard);
            var txtFlow = rootView.FindViewById<TextView>(Resource.Id.txtFlow);
            var txtRoom = rootView.FindViewById<TextView>(Resource.Id.txtRoom);

            txtCheckIn.Text = CheckInText;
            txtCase.Text = CaseText;
            txtCard.Text = CardText;
            txtFlow.Text = FlowText;
            txtRoom.Text = RoomText;

            return rootView;
        }

        private string CheckInText => string.Format("{0}\nSurgeon: {1}\nCirc: {2}",
            AppSettings.CurrentPatient.LastName,
            AppSettings.CurrentSurgery.SurgeonLastName,
            _circulatorName);

        private string CaseText => "Surgeon notes about the case";

        private string CardText => string.Format("{0} - Card: {1}", 
            AppSettings.CurrentCard.CardDescription,
            AppSettings.CurrentSurgery.CardID);

        private string FlowText => string.Format("{0} - Flow: {1}",
            AppSettings.CurrentFlow.Description,
            AppSettings.CurrentSurgery.FlowID);

        private string RoomText => string.Format("Room ID: {0}", AppSettings.CurrentRoom.RoomDescription);
    }
}