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
        private OpFlowTextPicker _surgeonPicker;

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
            swtDefault.ValueChanged += UpdateBundle;

            var surgeons = await UserUtil.GetSurgeons(null);
            _surgeonPicker = new OpFlowTextPicker(txtSurgeon, surgeons.Cast<IBindableEntity>().ToList());

            _surgeonPicker.ValueChanged += UpdateBundle;

            await UpdateCards();
        }

        private async void UpdateBundle(object sender, EventArgs e)
        {
            await UpdateCards();
        }

        private async Task UpdateCards()
        {
            var bundleId = _bundlePicker.GetCurrentId();
            var surgeonId = _surgeonPicker.GetCurrentId();
            var defaultFilter = swtDefault.On;

            var cards = await CardUtil.GetCards(bundleId, null, surgeonId, defaultFilter);
            var messagingTableViewSource = new CardListTVS(cards);

            var minimumCost = 0.0M;
            if (cards.Any())
                minimumCost = cards.Min(c => c.Cost);

            lblCardCost.Text = $"${minimumCost.ToString("#,##0.00")}";

            CardListTableView.RowHeight = 80f;
            CardListTableView.EstimatedRowHeight = 80f;

            CardListTableView.Source = messagingTableViewSource;
            CardListTableView.ReloadData();
        }
    }
}