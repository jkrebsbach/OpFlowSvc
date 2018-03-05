using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class AssignmentListViewController : UIViewController
    {
        public AssignmentListViewController (IntPtr handle) : base (handle)
        {
        }

        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();

            await LoadAssignmentOptions();
        }

        private async Task LoadAssignmentOptions()
        {
            var options = new List<IBindableEntity>();

            if (AppSettings.CurrentScreen == AppSettings.FragmentEnum.FlowAssignment)
            {
                var flows = await FlowUtil.GetProcedureFlows(AppSettings.CurrentCard ?? 0);
                options = flows.Cast<IBindableEntity>().ToList();
            }
            else
            {
                var cards = await CardUtil.GetCardsForProcedure(AppSettings.CurrentProcedure ?? 0);
                options = cards.Cast<IBindableEntity>().ToList();
            }

            var assignmentTVS = new AssignmentSelectionTVS(options);

            AssignmentSelectionTableView.Source = assignmentTVS;
            AssignmentSelectionTableView.ReloadData();
        }
    }
}