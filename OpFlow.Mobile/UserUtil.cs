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
            var command = "api/User/authorizedUser";
            
            var response = await WebUtility.WebRequest<User>(command, HttpMethod.Get);

            return response;
        }
        public static async Task<List<User>> GetUsers(string searchString)
        {
            var command = string.Format("api/user/searchUsers?nameSearchText={0}", searchString);

            var response = await WebUtility.WebRequest<List<User>>(command, HttpMethod.Get);

            return response;
        }
        public static async Task<List<Surgeon>> GetSurgeons(int specialtyId)
        {
            var command = string.Format("api/surgeon?specialtyId={0}", specialtyId);

            var response = await WebUtility.WebRequest<List<Surgeon>>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<string> CheckinUser(User user, int surgeryId)
        {
            var command = string.Format("api/user/checkin?surgeryId={0}", surgeryId);

            var response = await WebUtility.SendBodyRequest<string>(command, user, HttpMethod.Post);

            return response;
        }

        public static async Task<string> CheckoutUser(User user, int surgeryId)
        {
            var command = string.Format("api/user/checkout?surgeryId={0}", surgeryId);

            var response = await WebUtility.SendBodyRequest<string>(command, user, HttpMethod.Post);

            return response;
        }

        public static async Task<string> UserWorkflowReviewed(User user, int surgeryId)
        {
            var command = string.Format("api/user/reviewed?surgeryId={0}", surgeryId);

            var response = await WebUtility.SendBodyRequest<string>(command, user, HttpMethod.Post);

            return response;
        }
    }
}
