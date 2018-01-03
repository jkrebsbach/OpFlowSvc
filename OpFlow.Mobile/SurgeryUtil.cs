using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using OpFlow.Data;

namespace OpFlow.Mobile
{
    public static class SurgeryUtil
    {
        public static async Task<List<Surgery>> GetSurgerySchedule(DateTime scheduleDate)
        {
            var userId = AppSettings.CurrentUser.UserID;

            var command = string.Format("api/surgery?userId={0}", userId);
            var response = await WebUtility.WebRequest<List<Surgery>>(command, HttpMethod.Get);

            return response;
        }
        public static async Task<Surgery> GetSurgery(int surgeryId)
        {
            var command = string.Format("api/surgery/{0}", surgeryId);
            var response = await WebUtility.WebRequest<Surgery>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<List<SurgeryUser>> GetSurgeryUsers(int surgeryId)
        {
            var command = string.Format("api/surgery/{0}/users", surgeryId);
            var response = await WebUtility.WebRequest<List<SurgeryUser>>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<List<Surgery>> GetSurgeryRoomSchedule(int caseID, int providerID, int locationID)
        {
            var command = string.Format("api/surgery/RoomSchedule?caseID={0}&providerID={1}&locationID={2}", 
                caseID, providerID, locationID);

            var response = await WebUtility.WebRequest<List<Surgery>>(command, HttpMethod.Get);

            return response;
        }
    }
}
