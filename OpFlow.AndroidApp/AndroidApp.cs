using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using OpFlow.Data;
using Xamarin.Auth;

namespace OpFlow.AndroidApp
{
    public static class AndroidApp
    {
        private const string _appId = "OpFlow";

        public static async Task SetCredential(string userName, string password)
        {
            if (!string.IsNullOrWhiteSpace(userName))
            {
                var store = AccountStore.Create();
                var account = (await store.FindAccountsForServiceAsync(_appId)).FirstOrDefault() ?? new Account();

                account.Username = userName;
                account.Properties["Password"] = password;
                await store.SaveAsync(account, _appId);
            }
        }

        public static async Task<Account> GetCurrentCredential()
        {
            var store = AccountStore.Create();
            var length = store.FindAccountsForService(_appId).Count();
            var account = store.FindAccountsForService(_appId).FirstOrDefault();

            while (length > 1)
            {
                await store.DeleteAsync(account, _appId);

                length = store.FindAccountsForService(_appId).Count();
                account = store.FindAccountsForService(_appId).FirstOrDefault();
            }

            return account;
        }

    }
}