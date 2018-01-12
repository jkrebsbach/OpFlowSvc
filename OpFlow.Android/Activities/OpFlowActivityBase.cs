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
using OpFlow.Mobile;

namespace OpFlow.Android.Activities
{
    public abstract class OpFlowActivityBase : Activity
    {
        private Toolbar _toolbar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(GetLayoutResourceId());

            _toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
            //SetActionBar(toolbar);

            //toolbar.MenuItemClick += OnMenuItemClick;

            //ActionBar.Title = "";

            var editToolbar = FindViewById<Toolbar>(Resource.Id.nav_toolbar);
            editToolbar.Title = "";
            editToolbar.InflateMenu(Resource.Menu.nav_menu);
            editToolbar.MenuItemClick += (sender, e) => {
                NavbarClicked(e.Item);
            };
        }

        protected override void OnResume()
        {
            var appName = _toolbar.FindViewById<TextView>(Resource.Id.txtHeaderApplication);
            var userName = _toolbar.FindViewById<TextView>(Resource.Id.txtHeaderUser);

            appName.Text = AppSettings.CurrentScreen;
            userName.Text = AppSettings.CurrentUser?.LastName;

            base.OnResume();
        }

        //public override bool OnCreateOptionsMenu(IMenu menu)
        //{
        //    var inflater = new MenuInflater(this);
        //    inflater.Inflate(Resource.Menu.option_menu, menu);

        //    var actionShare = menu.FindItem(Resource.Id.menu_save);
        //    return base.OnCreateOptionsMenu(menu);
        //}

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

        protected abstract void NavbarClicked(IMenuItem menuItem);

        protected abstract int GetLayoutResourceId();
    }
}
