using Foundation;
using System;
using System.Threading.Tasks;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class DashboardViewController : OpFlowViewController
    {
        public DashboardViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            try
            {
                await ExecuteAsyncWebRequest(SetupDetails());

                DashboardTableView.RowHeight = 30f;
                DashboardTableView.EstimatedRowHeight = 30f;

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