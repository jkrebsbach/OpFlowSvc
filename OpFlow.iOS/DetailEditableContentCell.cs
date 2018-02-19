using Foundation;
using System;
using UIKit;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;

namespace OpFlow.iOS
{
    public partial class DetailEditableContentCell : DetailListCell
    {
        public DetailEditableContentCell (IntPtr handle) : base (handle)
        {
        }


        public override void UpdateCell(CaseDetailToken token)
        {
            txtDetail.Text = token.DetailText;

            txtDetail.Layer.BorderWidth = 2.0f;

            txtDetail.SetupDoneStyleTextBox();
        }
    }
}