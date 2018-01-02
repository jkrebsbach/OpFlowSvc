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

            var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            SetActionBar(toolbar);

            toolbar.MenuItemClick += OnMenuItemClick;

            ActionBar.Title = "IS THIS THING ON?";
            //SetTitle(Resource.String.empty_string);

            //toolbar.InflateMenu(Resource.Menu.option_menu);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var inflater = new MenuInflater(this);
            inflater.Inflate(Resource.Menu.option_menu, menu);

            var actionShare = menu.FindItem(Resource.Id.menu_save);
            return base.OnCreateOptionsMenu(menu);
        }

        public void OnMenuItemClick(object sender, Toolbar.MenuItemClickEventArgs e)
        {
            // Handle item selection
            switch (e.Item.ItemId)
            {
                case Resource.Id.menu_save:
                    //newGame();
                    return;
                case Resource.Id.menu_edit:
                    //showHelp();
                    return;
                default:
                    // No action
                    break;
            }
        }

        protected abstract int GetLayoutResourceId();
    }
}