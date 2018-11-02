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
using static OpFlow.iOS.SocketClient;

namespace OpFlow.iOS
{
    public partial class NavigateViewController : OpFlowViewController
    {
        private List<Messaging> _messages;

        Surgery _surgery;
        Patient _patient;

        bool _reviewNeeded;

        NSObject _notificationObserver;

        public NavigateViewController(IntPtr handle) : base(handle)
        {
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            AppDelegate.AppSocketClient.OnMessageReceived -= SocketMessageReceived;
            NSNotificationCenter.DefaultCenter.RemoveObserver(_notificationObserver);
        }

        public override async void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);


            _notificationObserver = NSNotificationCenter.DefaultCenter.AddObserver(UIApplication.DidBecomeActiveNotification, RefreshSocket);

            await AppDelegate.SetupSocketClient();
            await ExecuteAsyncWebRequest(LoadSurgery);

            AppDelegate.AppSocketClient.OnMessageReceived += SocketMessageReceived;
        }

        private async void RefreshSocket(NSNotification notification)
        {
            await AppDelegate.SetupSocketClient();
            await ExecuteAsyncWebRequest(LoadSurgery);
        }


        private void SocketMessageReceived(Object sender, MessageReceiveEvent message)
        {

            var newMessage = new Messaging()
            {
                SenderRoleID = (RoleEnum)message.SenderRoleID,
                UserName = message.SenderUserName,
                Message = message.Message,
                InsertTimestamp = message.InsertTimestamp
            };

            // Is this message part of the current conversation?
            if (message.SurgeryID == AppSettings.CurrentSurgery)
            {
                _messages.Add(newMessage);

                InvokeOnMainThread(() =>
                {
                    CommunicatorTableView.ReloadData();

                    var detailIndexPath = NSIndexPath.FromRowSection(_messages.Count - 1, 0);
                    CommunicatorTableView.ScrollToRow(detailIndexPath, UITableViewScrollPosition.None, true);

                });
            }
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            lblSurgeon.TextColor = RoleEnum.Surgeon.RoleBackgroundColorMapping();
            lblAnes.TextColor = RoleEnum.Anesthesiologist.RoleBackgroundColorMapping();
            lblCirculator.TextColor = RoleEnum.Circulator.RoleBackgroundColorMapping();
            lblScrub.TextColor = RoleEnum.ScrubTech.RoleBackgroundColorMapping();
            lblCrna.TextColor = RoleEnum.CRNA.RoleBackgroundColorMapping();

            SetupTitleView(vwSurgeon);
            SetupTitleView(vwCirculator);
            SetupTitleView(vwAnes);
            SetupTitleView(vwScrub);
            SetupTitleView(vwRep);

            SetupTitleView(CommunicatorTableView);

            SetupDoneStyleTextField(txtCommunicator);

            Title = "SCHEDULEVIEW";

        }

        async partial void btnSendMessage_Click(UIButton sender)
        {
            if (txtCommunicator.Text == "")
                return;

            await AppDelegate.AppSocketClient.SendSurgeryMessage(AppSettings.CurrentSurgery.Value, txtCommunicator.Text);
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

        async partial void btnDebrief_Click(UIButton sender)
        {
            if (_reviewNeeded)
            {
                await ExecuteAsyncWebRequest(async () => await SurgeryUtil.ReviewSurgery(_surgery.SurgeryID));
                await ExecuteAsyncWebRequest(LoadSurgery);

            }
            else
            {
                NavigateScreen(AppSettings.FragmentEnum.Debrief);
            }
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
            var currentUser = users.FirstOrDefault(u => u.RoleID == 1 &&
                                                   u.UserID == AppSettings.CurrentUser.UserID);
            if (currentUser != null && currentUser.SurgeryReview == null)
            {
                btnDebrief.Hidden = false;
                btnDebrief.SetTitle("REVIEW", UIControlState.Normal);
                btnDebrief.BackgroundColor = UIColor.Red;

                _reviewNeeded = true;
            }
            else 
            {
                btnDebrief.Hidden = (_surgery.SurgeryStatus != "O");
            }

            lblSurgeonName.Text = GetUserName(users, RoleEnum.Surgeon);
            lblCirculatorName.Text = GetUserName(users, RoleEnum.Circulator);
            lblScrubName.Text = GetUserName(users, RoleEnum.ScrubTech);
            lblAnesName.Text = GetUserName(users, RoleEnum.FrontDesk);
            lblCrnaName.Text = GetUserName(users, RoleEnum.CRNA);

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
            _messages = await MessagingUtil.GetMessages(_surgery.SurgeryID, null, null);

            var messagingTableViewSource = new CommunicatorTVS(_messages);

            CommunicatorTableView.Source = messagingTableViewSource;
            CommunicatorTableView.ReloadData();

            // If we have any messages, scroll to bottom of message stack
            if (_messages.Count > 0)
            {
                var detailIndexPath = NSIndexPath.FromRowSection(_messages.Count - 1, 0);
                CommunicatorTableView.ScrollToRow(detailIndexPath, UITableViewScrollPosition.None, true);
            }
        }

        private string GetUserName(List<SurgeryUser> users, RoleEnum roleId)
        {
            var user = users.FirstOrDefault(u => u.RoleID == (int)roleId);

            return user == null ? "NONE" : string.Format("{0}, {1} {2}", user.LastName, user.FirstName, user.UserTitle);
        }

        private string PatientNameText =>  
            $"{_patient?.LastName ?? ""}, {_patient?.FirstName ?? ""} {_patient.MiddleInitial}";

        private string PatientInfoText => string.Format("{0} {1}", _patient?.BirthDate.CalculateAge(), _patient?.Gender);
    }
}