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

            lblFlowDescription.Text = flow?.FlowDescription;
        }
    }
}