using Foundation;
using System;
using System.Threading.Tasks;
using OpFlow.iOS.Delegates;
using UIKit;

namespace OpFlow.iOS
{
    public partial class ContainerViewController : UIViewController
    {
        private TaskCompletionSource<bool> _viewChanging;
        private readonly NSString ScheduleSegue = (NSString)"scheduleSegue";
        private readonly NSString DetailSegue = (NSString)"detailSegue";

        private INavigationDelegate _hostController;

        public ContainerViewController (IntPtr handle) : base (handle)
        {
        }

        public void SetupHost(INavigationDelegate hostController)
        {
            _hostController = hostController;
        }

        public TaskCompletionSource<bool> ViewChanging => _viewChanging;

        public Task<bool> PresentScheduleViewAsync()
        {
            _viewChanging = new TaskCompletionSource<bool>();
            PerformSegue(ScheduleSegue, this);

            return _viewChanging.Task;
        }

        public Task<bool> PresentDetailViewAsync()
        {
            _viewChanging = new TaskCompletionSource<bool>();
            PerformSegue(DetailSegue, this);

            return _viewChanging.Task;
        }

        #region Segue Operations

        public override void PrepareForSegue(UIStoryboardSegue segue,
            NSObject sender)
        {
            // validate segue
            if ((segue.Identifier != ScheduleSegue) && 
                (segue.Identifier != DetailSegue)) return;

            if (segue.DestinationViewController is INavigationTargetDelegate targetScene)
            {
                targetScene.NavigationDelegate = _hostController;
            }

            if (ChildViewControllers.Length > 0)
            {
                SwapFromViewController(ChildViewControllers[0],
                    segue.DestinationViewController);
            }
            else
            {
                AddInitialViewController(segue.DestinationViewController);
            }
        }

        private void AddInitialViewController(UIViewController viewController)
        {
            //on first run no transition animation
            AddChildViewController(viewController);

            viewController.View.Frame = View.Bounds;

            Add(viewController.View);

            viewController.DidMoveToParentViewController(this);

            _viewChanging.TrySetResult(true);
        }

        private void SwapFromViewController(UIViewController fromViewController,
            UIViewController toViewController)
        {
            fromViewController.WillMoveToParentViewController(null);

            toViewController.View.Frame = this.View.Bounds;

            AddChildViewController(toViewController);

            Transition(fromViewController,
                toViewController,
                0.3,
                UIViewAnimationOptions.TransitionCrossDissolve,
                () => { },
                (bool finished) =>
                {
                    fromViewController.RemoveFromParentViewController();
                    toViewController.DidMoveToParentViewController(this);

                    _viewChanging.TrySetResult(true);
                });
        }

        #endregion
    }
}