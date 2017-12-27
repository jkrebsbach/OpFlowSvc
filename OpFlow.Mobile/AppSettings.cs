using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public static class AppSettings
    {
        private static AuthToken _authToken;
        public static Surgeon CurrentSurgeon { get; private set; }

        public static bool UserAuthenticated => _authToken != null && _authToken.ExpiresDate > DateTime.Now;

        public static async Task AuthenticateUser(string username, string password)
        {
            username = "OpFlow@OpFlow.com";
            password = "OpFlow1!";

            _authToken = await WebUtility.LoginUser(username, password);

            if (_authToken != null)
                CurrentSurgeon = await SurgeonUtil.GetSurgeon(username);
        }
    }
}
