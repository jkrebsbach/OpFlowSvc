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

namespace OpFlow.Android.Activities
{
    public abstract class OpFlowActivityBase : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(GetLayoutResourceId());

            SetTitle(Resource.String.empty_string);

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);

            toolbar.InflateMenu(Resource.Menu.option_menu);
        }

        protected abstract int GetLayoutResourceId();
    }
}