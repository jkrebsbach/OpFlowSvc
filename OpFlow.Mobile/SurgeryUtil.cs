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
            var providerId = AppSettings.CurrentUser.ProviderID;
            var locationId = AppSettings.CurrentUser.LocationID;

            var command = string.Format("api/surgery/cases?userId={0}&providerId={1}&locationId={2}", userId, providerId, locationId);
            var response = await WebUtility.WebRequest<List<Surgery>>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<Dictionary<int, Patient>> GetSurgeryPatients(List<Surgery> surgeries)
        {
            var result = new Dictionary<int, Patient>();

            foreach (var surgery in surgeries)
            {
                result[surgery.SurgeryID] = await PatientUtil.GetPatient(surgery.PatientID);
            }

            return result;
        }

        public static async Task<Surgery> GetSurgery(int surgeryId, int providerId, int locationId)
        {
            var command = string.Format("api/surgery?surgeryId={0}&providerId={1}&locationId={2}",
                surgeryId, providerId, locationId);
            var response = await WebUtility.WebRequest<Surgery>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<List<SurgeryUser>> GetSurgeryUsers(int caseId, int providerId, int locationId)
        {
            var command = string.Format("api/surgery/users?caseId={0}&providerId={1}&locationId={2}", 
                caseId, providerId, locationId);
            var response = await WebUtility.WebRequest<List<SurgeryUser>>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<List<User>> GetCardUsers(int cardId, int providerId, int locationId)
        {
            var command = string.Format("api/card/users?cardId={0}&providerId={1}&locationId={2}",
                cardId, providerId, locationId);
            var response = await WebUtility.WebRequest<List<User>>(command, HttpMethod.Get);

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
