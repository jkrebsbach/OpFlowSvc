using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public static class UserUtil
    {
        public static async Task<User> GetUser(string username)
        {
            var command = string.Format("api/userbyusername?username={0}", username);
            
            var response = await WebUtility.WebRequest<User>(command, HttpMethod.Get);

            return response;
        }
    }
}
