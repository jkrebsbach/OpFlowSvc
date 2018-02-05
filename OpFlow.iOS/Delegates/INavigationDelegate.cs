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

        void PresentContainerView(AppSettings.FragmentEnum fragmentEnum);
    }
}