using Foundation;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace OpFlow.iOS
{
    public partial class CardListViewController : OpFlowViewController
    {
        private OpFlowTextPicker _bundlePicker;

        public CardListViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            try
            {
                await ExecuteAsyncWebRequest(SetupCardList);
                //await SetupDetails();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private async Task SetupCardList()
        {
            var bundles = await LookupUtil.GetBundles(null);
            _bundlePicker = new OpFlowTextPicker(txtBundle, bundles.Cast<IBindableEntity>().ToList());

            _bundlePicker.ValueChanged += UpdateBundle;

            await UpdateCards();
        }

        private async void UpdateBundle(object sender, EventArgs e)
        {
            await UpdateCards();
        }

        private async Task UpdateCards()
        {
            var bundleId = _bundlePicker.GetCurrentId();

            var cards = await CardUtil.GetCards(bundleId, null);
            var messagingTableViewSource = new CardListTVS(cards);

            CardListTableView.Source = messagingTableViewSource;
            CardListTableView.ReloadData();
        }
    }
}