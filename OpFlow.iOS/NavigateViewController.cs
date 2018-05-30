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
    public partial class NavigateViewController : OpFlowViewController
    {
        
        Surgery _surgery;
        Patient _patient;
        
        public NavigateViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            lblSurgeon.TextColor = RoleEnum.Surgeon.RoleBackgroundColorMapping();
            lblAnes.TextColor = RoleEnum.Anesthesiologist.RoleBackgroundColorMapping();
            lblCirculator.TextColor = RoleEnum.Circulator.RoleBackgroundColorMapping();
            lblScrub.TextColor = RoleEnum.ScrubTech.RoleBackgroundColorMapping();
            lblRep.TextColor = RoleEnum.Representative.RoleBackgroundColorMapping();

            SetupTitleView(vwSurgeon);
            SetupTitleView(vwCirculator);
            SetupTitleView(vwAnes);
            SetupTitleView(vwScrub);
            SetupTitleView(vwRep);

            SetupTitleView(CommunicatorTableView);

            SetupDoneStyleTextField(txtCommunicator);

            Title = "SCHEDULEVIEW";
            await ExecuteAsyncWebRequest(LoadSurgery);
            //await LoadSurgery();
        }

        async partial void btnRefresh_Click(UIButton sender)
        {
            await ExecuteAsyncWebRequest(LoadMessages);
        }

        async partial void btnSendMessage_Click(UIButton sender)
        {
            if (txtCommunicator.Text == "")
                return;

            var messageGroup = new MessagingGroup()
            {
                SurgeryID = AppSettings.CurrentSurgery
            };

            await MessagingUtil.SendMessage(messageGroup, txtCommunicator.Text);
            txtCommunicator.ResignFirstResponder();

            txtCommunicator.Text = string.Empty;

            await LoadMessages();
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
            if (_surgery.CardID == null)
            {
                NavigateScreen(AppSettings.FragmentEnum.CardAssignment);
                return;
            }
            NavigateScreen(AppSettings.FragmentEnum.CardDetail);
        }

        partial void btnFlow_Click(UIButton sender)
        {
            if (_surgery.FlowID == null)
            {
                NavigateScreen(AppSettings.FragmentEnum.FlowAssignment);
                return;
            }
            NavigateScreen(AppSettings.FragmentEnum.FlowDetail);
        }

        partial void btnDashboard_Click(UIButton sender)
        {
            NavigateScreen(AppSettings.FragmentEnum.Dashboard);
        }

        partial void btnRoom_Click(UIButton sender)
        {
            NavigateScreen(AppSettings.FragmentEnum.Room);
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
                AppSettings.CurrentSurgery ?? 0);

            if (_surgery == null)
                return;

            AppSettings.LoadSurgeryAttributes(_surgery);

            _patient = await PatientUtil.GetPatient(_surgery.PatientID);

            var users = await SurgeryUtil.GetSurgeryUsers(_surgery.SurgeryID);

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
            lblFlowStep.Text = _surgery.FlowDescription;

            await LoadMessages();
        }

        private async Task LoadMessages()
        {
            var messaging = await MessagingUtil.GetMessages(_surgery.SurgeryID, null, null);

            var messagingTableViewSource = new CommunicatorTVS(messaging);

            CommunicatorTableView.Source = messagingTableViewSource;
            CommunicatorTableView.ReloadData();

            // If we have any messages, scroll to bottom of message stack
            if (messaging.Count > 0)
            {
                var detailIndexPath = NSIndexPath.FromRowSection(messaging.Count - 1, 0);
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
    }
}