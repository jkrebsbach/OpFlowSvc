using Foundation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class DetailViewController : UIViewController
    {
        public DetailViewController (IntPtr handle) : base (handle)
        {
            
        }

        public override async void ViewDidLoad()
        {
            try
            {
                await SetupDetails();

                DetailTableView.ReloadData();
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

            var patient = await PatientUtil.GetPatient(surgery.PatientID);

            var tokens = await CaseDetailToken.GetCaseDetailTokens(surgery, patient);

            var detailTableViewSource = new DetailListTVS(tokens);

            DetailTableView.Source = detailTableViewSource;
        }
    }
}