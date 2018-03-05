using Foundation;
using System;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using UIKit;

namespace OpFlow.iOS
{
    public partial class SelectCardCell : BindableTableViewCell
    {
        public SelectCardCell (IntPtr handle) : base (handle)
        {
        }

        public override void UpdateCell(IBindableEntity entity)
        {
            var card = entity as Card;
            if (card == null)
                return;

            lblCardDetail.Text = card.CardDescription;
            lblCardSummary.Text = $"Avg Time:{card.AvgMinutes} Cost: ${card.Cost}";
        }
    }
}