using Foundation;
using System;
using UIKit;
using System.Threading.Tasks;
using OpFlow.Mobile;
using OpFlow.Data;
using System.Collections.Generic;
using System.Linq;
using OpFlow.iOS.Delegates;

namespace OpFlow.iOS
{
    public partial class NavigateViewController : UIViewController, INavigationTargetDelegate
    {
        Surgery _surgery;
        Flow _flow;
        Patient _patient;

        public NavigateViewController (IntPtr handle) : base (handle)
        {
        }

        public INavigationDelegate NavigationDelegate { get; set; }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            vwSurgeon.Layer.BorderWidth = 1.0f;
            vwSurgeon.Layer.BorderColor = UIColor.Black.CGColor;

            vwSurgeon.LayoutMargins = new UIEdgeInsets(2, 2, 2, 2);

            Title = "SCHEDULEVIEW";
            await LoadSurgery();
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
            lblDebrief.Hidden = (_surgery.SurgeryStatus != "O");

            lblSurgeonName.Text = GetUserName(users, AppSettings.RoleEnum.Surgeon);
            lblCirculatorName.Text = GetUserName(users, AppSettings.RoleEnum.Circulator);
            lblScrubName.Text = GetUserName(users, AppSettings.RoleEnum.ScrubTech);
            lblAnesName.Text = GetUserName(users, AppSettings.RoleEnum.FrontDesk);
            lblRepName.Text = GetUserName(users, AppSettings.RoleEnum.FrontDesk);

            lblPatientName.Text = PatientNameText;
            lblPatientInfo.Text = PatientInfoText;
            lblSurgeryTime.Text = _surgery.ScheduleTime.ToString(@"hh\:mm");
        }

        private string GetUserName(List<SurgeryUser> users, AppSettings.RoleEnum roleId)
        {
            var user = users.FirstOrDefault(u => u.RoleID == (int)roleId);

            return user == null ? "NONE" : string.Format("{0}, {1} {2}", user.LastName, user.FirstName, user.Title);
        }

        private string PatientNameText => string.Format("{0}, {1}", _patient?.LastName, _patient?.FirstName);

        private string PatientInfoText => string.Format("{0} {1}", _patient?.BirthDate.CalculateAge(), _patient?.Gender);


        private string FlowText => string.Format("{0} - Flow: {1}",
            _flow.Description,
            _surgery.FlowID);
    }
}