using Foundation;
using System;
using System.Collections.Generic;
using OpFlow.Data;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class ScheduleViewController : UIViewController
    {
        public ScheduleViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            var schedule = await SurgeryUtil.GetSurgeryUserSchedule(DateTime.Today);

            ScheduleTableView.Source = new SurgeryTVS(schedule);
        }
    }
}