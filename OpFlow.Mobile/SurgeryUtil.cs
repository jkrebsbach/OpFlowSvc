using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

using OpFlow.Data;

namespace OpFlow.Mobile
{
    public abstract class SurgeryUtil
    {
        public static async Task<List<Surgery>> GetSurgeryUserSchedule(DateTime scheduleDate)
        {
            var userId = AppSettings.CurrentUser.UserID;
            var providerId = AppSettings.CurrentUser.ProviderID;
            var locationId = AppSettings.CurrentUser.LocationID;

            var command = string.Format("api/surgery/cases?userId={0}&providerId={1}&locationId={2}", userId, providerId, locationId);
            
            var response = await WebUtility.WebRequest<List<Surgery>>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<List<Surgery>> GetSurgeryRoomSchedule(int roomId)
        {
            var providerId = AppSettings.CurrentUser.ProviderID;
            var locationId = AppSettings.CurrentUser.LocationID;

            var command = string.Format("api/Surgery/RoomSchedule?roomId={0}&providerId={1}&locationId={2}", roomId, providerId, locationId);
            var response = await WebUtility.WebRequest<List<Surgery>>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<Dictionary<int, Patient>> GetSurgeryPatients(List<Surgery> surgeries)
        {
            var result = new Dictionary<int, Patient>();

            var patientIds = surgeries.GroupBy(s => s.PatientID).Select(g => g.Key).ToList();
            var patients = await PatientUtil.GetPatients(patientIds);

            foreach (var surgery in surgeries)
            {
                result[surgery.SurgeryID] = patients.FirstOrDefault(p => p.PatientID == surgery.PatientID);
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

        public static async Task<List<SurgeryUser>> GetSurgeryUsers(int caseId)
        {
            var providerId = AppSettings.CurrentUser.ProviderID;
            var locationId = AppSettings.CurrentUser.LocationID;

            var command = string.Format("api/surgery/users?caseId={0}&providerId={1}&locationId={2}", 
                caseId, providerId, locationId);
            var response = await WebUtility.WebRequest<List<SurgeryUser>>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<List<SurgeryUser>> GetCardUsers(int cardId)
        {
            var providerId = AppSettings.CurrentUser.ProviderID;
            var locationId = AppSettings.CurrentUser.LocationID;

            var command = string.Format("api/card/users?cardId={0}&providerId={1}&locationId={2}",
                cardId, providerId, locationId);
            var response = await WebUtility.WebRequest<List<SurgeryUser>>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<List<Surgery>> GetSurgeryCaseRoomSchedule(int caseID)
        {
            var providerId = AppSettings.CurrentUser.ProviderID;
            var locationId = AppSettings.CurrentUser.LocationID;

            var command = string.Format("api/surgery/RoomSchedule?caseID={0}&providerID={1}&locationID={2}", 
                caseID, providerId, locationId);

            var response = await WebUtility.WebRequest<List<Surgery>>(command, HttpMethod.Get);

            return response;
        }
    }
}
