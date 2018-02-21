using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

using OpFlow.Mobile;

namespace OpFlow.iOS.Delegates
{
    public interface INavigationDelegate
    {
        event EventHandler DoneEventFired;

        void PresentContainerView(AppSettings.FragmentEnum fragmentEnum);
    }
}