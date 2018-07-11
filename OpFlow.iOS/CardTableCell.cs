using Foundation;
using OpFlow.Data;
using System;
using UIKit;

namespace OpFlow.iOS
{
    public partial class CardTableCell : UITableViewCell
    {
        public CardTableCell(IntPtr handle) : base(handle)
        {
        }

        public void UpdateCell(Card card)
        {
            lblProcedureName.Text = $"{card.CptCode} {card.CardDescription}";
            lblProcedureDetail.Text = $"Avg Time: {card.AverageTime} Time Cost: {card.TimeCost}";
            lblProcedureCost.Text = $"Card Cost: {card.Cost} Total: {card.TotalCost}";
        }
    }
}