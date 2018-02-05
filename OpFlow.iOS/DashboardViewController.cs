using Foundation;
using System;
using System.Threading.Tasks;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class DashboardViewController : UIViewController, INavigationTargetDelegate
    {
        public DashboardViewController (IntPtr handle) : base (handle)
        {
        }

        public INavigationDelegate NavigationDelegate { get; set; }

        public override async void ViewDidLoad()
        {
            try
            {
                await SetupDetails();

                DashboardTableView.RowHeight = 20f;
                DashboardTableView.EstimatedRowHeight = 20f;

                DashboardTableView.ReloadData();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task SetupDetails()
        {
            var surgery = await SurgeryUtil.GetSurgery(
                AppSettings.CurrentSurgery ?? 0,
                AppSettings.CurrentUser.ProviderID,
                AppSettings.CurrentUser.LocationID);

            if (surgery == null)
                return;

            var flowTimings = await FlowUtil.GetFlowTimings(surgery.FlowID);

            var flowTimingTableViewSource = new DashboardTVS(flowTimings);
            
            DashboardTableView.Source = flowTimingTableViewSource;
        }
    }
}