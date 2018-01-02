using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpFlow.Android.Activities;

namespace OpFlow.Android.Fragments
{
    public class OpFlowFragmentBase : Fragment
    {
        protected IFragmentMessageListener Listener { get; private set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            Listener = Activity as IFragmentMessageListener;
            if (Listener == null)
            {
                throw new InvalidCastException("Activity must implement IFragmentMessageListener");
            }

            return base.OnCreateView(inflater, container, savedInstanceState);
        }
    }
}