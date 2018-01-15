using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpFlow.Mobile;

namespace OpFlow.AndroidApp.Activities
{
    public interface IFragmentMessageListener
    {
        void SendMessage(AppSettings.FragmentEnum fragment, object payload);

        void UpdateToolbar();
    }
}