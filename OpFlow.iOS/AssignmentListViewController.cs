using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.iOS.ViewSources;
using OpFlow.Mobile;
using UIKit;

namespace OpFlow.iOS
{
    public partial class AssignmentListViewController : OpFlowViewController
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
            List<IBindableEntity> options;
            int currentId;

            if (AppSettings.CurrentScreen == AppSettings.FragmentEnum.FlowAssignment)
            {
                currentId = AppSettings.CurrentFlow ?? -1;

                var flows = await FlowUtil.GetCardFlows(AppSettings.CurrentCard ?? 0);
                options = flows.Cast<IBindableEntity>().ToList();
            }
            else
            {
                currentId = AppSettings.CurrentCard ?? -1;

                var cards = await CardUtil.GetCards(AppSettings.CurrentProcedure ?? 0);
                options = cards.Cast<IBindableEntity>().ToList();
            }

            var assignmentTVS = new AssignmentSelectionTVS(options);

            AssignmentSelectionTableView.Source = assignmentTVS;

            AssignmentSelectionTableView.ReloadData();

            var selectedItem = options.FirstOrDefault(o => o.GetID() == currentId);
            if (selectedItem != null)
            {
                var selectionIndex = options.IndexOf(selectedItem);

                AssignmentSelectionTableView.SelectRow(NSIndexPath.FromRowSection(selectionIndex, 0), true, UITableViewScrollPosition.Bottom);
            }

            assignmentTVS.EntitySelectionEvent += SelectEntity;
        }

        private async void SelectEntity(object sender, IBindableEntity entity)
        {
            if (entity is Flow)
            {
                await SurgeryUtil.AssignFlow(AppSettings.CurrentSurgery ?? -1, entity.GetID());
                AppSettings.CurrentFlow = entity.GetID();

                NavigationDelegate?.PresentContainerView(AppSettings.FragmentEnum.FlowDetail);
            }
            else
            {
                await SurgeryUtil.AssignCard(AppSettings.CurrentSurgery ?? -1, entity.GetID());
                AppSettings.CurrentCard = entity.GetID();

                NavigationDelegate?.PresentContainerView(AppSettings.FragmentEnum.CardDetail);
            }

        }
    }
}