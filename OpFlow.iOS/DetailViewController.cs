using Foundation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class DetailViewController : OpFlowViewController
    {
        public DetailViewController (IntPtr handle) : base (handle)
        {
            
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            try
            {
                await ExecuteAsyncWebRequest(SetupDetails);
                //await SetupDetails();

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
                AppSettings.CurrentSurgery ?? 0);

            if (surgery == null)
                return;

            var patient = await PatientUtil.GetPatient(surgery.PatientID);

            var categories = await CaseDetailCategory.GetCaseDetailTokens(surgery, patient);
            var allowEdit = CaseDetailToken.AllowEdit();

            var detailTableViewSource = new DetailListTVS(categories, allowEdit);
            detailTableViewSource.DetailListConfirmedEvent += ConfirmDetails;

            DetailTableView.Source = detailTableViewSource;
        }

        private async void ConfirmDetails(object sender, List<DetailItem> tokens)
        {
            // Save changes to tokens

            NavigationDelegate?.PresentContainerView(AppSettings.PriorScreen);
        }
    }
}