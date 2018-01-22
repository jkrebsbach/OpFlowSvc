using System;
using System.Collections.Generic;
using System.Text;

namespace OpFlow.iOS.Delegates
{
    public interface INavigationTargetDelegate
    {
        INavigationDelegate NavigationDelegate { get; set; }
    }
}
