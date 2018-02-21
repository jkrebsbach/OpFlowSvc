using Foundation;
using System;
using UIKit;
using System.Threading.Tasks;
using OpFlow.Mobile;
using OpFlow.Data;
using System.Collections.Generic;
using System.Linq;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;

namespace OpFlow.iOS
{
    public partial class NavigateViewController : OpFlowViewController, INavigationTargetDelegate
    {
        async partial void btnSendMessage_Click(UIButton sender)
        {
            await MessagingUtil.SendMessage(txtCommunicator.Text);

            txtCommunicator.Text = string.Empty;
        }

        partial void btnCommunicator_Click(UIButton sender)
        {
            NavigateScreen(AppSettings.FragmentEnum.Communicator);
        }

        partial void btnPatient_Click(UIButton sender)
        {
            NavigateScreen(AppSettings.FragmentEnum.Patient);
        }

        partial void btnCard_Click(UIButton sender)
        {
            NavigateScreen(AppSettings.FragmentEnum.CardDetail);
        }

        partial void btnFlow_Click(UIButton sender)
        {
            NavigateScreen(AppSettings.FragmentEnum.FlowDetail);
        }

        partial void btnDashboard_Click(UIButton sender)
        {
            NavigateScreen(AppSettings.FragmentEnum.Dashboard);
        }

        Surgery _surgery;
        Flow _flow;
        Patient _patient;
        List<Messaging> _messaging;

        public NavigateViewController (IntPtr handle) : base (handle)
        {
        }

        public INavigationDelegate NavigationDelegate { get; set; }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            SetupTitleView(vwSurgeon);
            SetupTitleView(vwCirculator);
            SetupTitleView(vwAnes);
            SetupTitleView(vwScrub);
            SetupTitleView(vwRep);

            SetupTitleView(CommunicatorTableView);

            txtCommunicator.SetupDoneStyleTextField();

            Title = "SCHEDULEVIEW"; 
            await LoadSurgery();
        }

        partial void btnDebrief_Click(UIButton sender)
        {
            NavigateScreen(AppSettings.FragmentEnum.Debrief);
        }

        private void NavigateScreen(AppSettings.FragmentEnum targetScreen)
        {
            NavigationDelegate?.PresentContainerView(targetScreen);
        }

        private void SetupTitleView(UIView view)
        {

            view.Layer.BorderWidth = 1.0f;
            view.Layer.BorderColor = UIColor.Black.CGColor;

            view.LayoutMargins = new UIEdgeInsets(2, 2, 2, 2);   
        }


        private async Task LoadSurgery()
        {
            _surgery = await SurgeryUtil.GetSurgery(
                AppSettings.CurrentSurgery ?? 0, 
                AppSettings.CurrentUser.ProviderID, 
                AppSettings.CurrentUser.LocationID);

            if (_surgery == null)
                return;
            
            _patient = await PatientUtil.GetPatient(_surgery.PatientID);
            _flow = await FlowUtil.GetFlow(_surgery.FlowID, _surgery.CardID);
            var users = await SurgeryUtil.GetSurgeryUsers(_surgery.CaseID,
                                                         AppSettings.CurrentUser.ProviderID,
                                                          AppSettings.CurrentUser.LocationID);

            // If case is open, show debrief button
            btnDebrief.Hidden = (_surgery.SurgeryStatus != "O");

            lblSurgeonName.Text = GetUserName(users, RoleEnum.Surgeon);
            lblCirculatorName.Text = GetUserName(users, RoleEnum.Circulator);
            lblScrubName.Text = GetUserName(users, RoleEnum.ScrubTech);
            lblAnesName.Text = GetUserName(users, RoleEnum.FrontDesk);
            lblRepName.Text = GetUserName(users, RoleEnum.FrontDesk);

            lblPatientName.Text = PatientNameText;
            lblPatientInfo.Text = PatientInfoText;
            lblProcedure.Text = _surgery.ProcedureDescription;
            lblSurgeryTime.Text = _surgery.ScheduleTime.ToString(@"hh\:mm");
            lblLocation.Text = _surgery.RoomDescription;
            lblFlowStep.Text = _flow.FlowDescription;


            _messaging = await MessagingUtil.GetMessages(_surgery.CaseID, null, null);

            var messagingTableViewSource = new CommunicatorTVS(_messaging);

            CommunicatorTableView.Source = messagingTableViewSource;
            CommunicatorTableView.ReloadData();

            // If we have any messages, scroll to bottom of message stack
            if (_messaging.Count > 0)
            {
                var detailIndexPath = NSIndexPath.FromRowSection(_messaging.Count - 1, 0);
                CommunicatorTableView.ScrollToRow(detailIndexPath, UITableViewScrollPosition.None, true);
            }
        }

        private string GetUserName(List<SurgeryUser> users, RoleEnum roleId)
        {
            var user = users.FirstOrDefault(u => u.RoleID == (int)roleId);

            return user == null ? "NONE" : string.Format("{0}, {1} {2}", user.LastName, user.FirstName, user.UserTitle);
        }

        private string PatientNameText =>  string.IsNullOrEmpty(_patient?.Initials) ? "UNK" : _patient.Initials;

        private string PatientInfoText => string.Format("{0} {1}", _patient?.BirthDate.CalculateAge(), _patient?.Gender);


        private string FlowText => string.Format("{0} - Flow: {1}",
            _flow.FlowDescription,
            _surgery.FlowID);
    }
}