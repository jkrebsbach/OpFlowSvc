using Foundation;
using OpFlow.Data;
using OpFlow.iOS.Delegates;
using OpFlow.Mobile;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace OpFlow.iOS
{
    public partial class ImageSetupViewController : OpFlowViewController
    {
        private OpFlowTextPicker _stepPicker;
        private OpFlowTextPicker _rolePicker;

        public ImageSetupViewController (IntPtr handle) : base (handle)
        {
            // done button click should fire this:
            //await UploadImage(evt.EditedImage);
        }
        public override async void ViewDidLoad()
        {
            base.ViewDidLoad();


            await ExecuteAsyncWebRequest(SetupDetails);
        }

        private async Task SetupDetails()
        {
            var flowId = AppSettings.CurrentFlow ?? 0;

            var steps = await FlowUtil.GetFlowTimings(flowId);
            _stepPicker = new OpFlowTextPicker(txtStep, steps.Cast<IBindableEntity>().ToList());

            var roles = await UserUtil.GetRoles();
            _rolePicker = new OpFlowTextPicker(txtRole, roles.Cast<IBindableEntity>().ToList());
        }

        async partial void btnAccept_Click(UIButton sender)
        {
            var stepId = _stepPicker.GetCurrentId();
            var roleId = _rolePicker.GetCurrentId();

            if ((stepId ?? 0) == 0)
            {
                ShowDialog("No step", "Please select a step");
                return;
            }

            if ((roleId ?? 0) == 0)
            {
                ShowDialog("No role", "Please select a role");
                return;
            }

            AppSettings.PriorScreen = AppSettings.CurrentScreen;
            var flowId = AppSettings.CurrentFlow ?? 0;
            await FlowUtil.UploadFlowImage(flowId, stepId.Value, roleId.Value, AppSettings.CurrentImage);

            AppSettings.CurrentImage = null;
            // Navigate to flow detail page

            NavigationDelegate?.PresentContainerView(AppSettings.FragmentEnum.FlowDetail);
        }

    }
}