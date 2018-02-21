using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public abstract class UserUtil
    {
        public static async Task<User> GetUser()
        {
            var command = "api/authorizedUser";
            
            var response = await WebUtility.WebRequest<User>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<string> CheckinUser(User user, int surgeryId)
        {
            var command = string.Format("api/user/checkin?surgeryId={0}", surgeryId);

            var response = await WebUtility.PostBodyRequest<string>(command, user);

            return response;
        }

        public static async Task<string> CheckoutUser(User user, int surgeryId)
        {
            var command = string.Format("api/user/checkout?surgeryId={0}", surgeryId);

            var response = await WebUtility.PostBodyRequest<string>(command, user);

            return response;
        }

        public static async Task<string> UserWorkflowReviewed(User user, int surgeryId)
        {
            var command = string.Format("api/user/reviewed?surgeryId={0}", surgeryId);

            var response = await WebUtility.PostBodyRequest<string>(command, user);

            return response;
        }
    }
}
