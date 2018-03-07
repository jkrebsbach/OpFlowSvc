using Foundation;
using System;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using UIKit;

namespace OpFlow.iOS
{
    public partial class SelectFlowCell : BindableTableViewCell
    {
        public SelectFlowCell (IntPtr handle) : base (handle)
        {
        }

        public override void UpdateCell(IBindableEntity entity)
        {
            var flow = entity as Flow;
            if (flow == null)
                return;

            lblFlowDescription.Text = $"{flow?.FlowDescription} - {flow?.OwnerLastName}";
            lblFlowSummary.Text = $"Avg Time: {flow?.AvgMinutes} Used: {flow?.TimesUsed}";
        }
    }
}