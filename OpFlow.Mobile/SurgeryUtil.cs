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
        public static async Task<List<SurgerySchedule>> GetSurgeryUserSchedule(DateTime scheduleDate, int? roomId = null)
        {
            var userId = AppSettings.CurrentUser.UserID;

            var command = $"api/surgery/cases?userId={userId}&scheduleDate={scheduleDate:yyyy-MM-dd}";
            if (roomId.HasValue)
                command += $"&roomId={roomId}";
            
            var response = await WebUtility.WebRequest<List<SurgerySchedule>>(command, HttpMethod.Get);

            return response;
        }
        public static async Task<List<SurgerySearchResult>> SearchCases(string caseNbr, int? surgeonUserId, int? roomId,
            DateTime? beginDate, DateTime? endDate)
        {
            var command = $"api/surgery/searchCases?";
            var strDelim = string.Empty;

            if (caseNbr != null)
            {
                command += $"{strDelim}caseNbr={caseNbr}";
                strDelim = "&";
            }
            if (roomId.HasValue)
            {
                command += $"{strDelim}roomId={roomId}";
                strDelim = "&";
            }
            if (surgeonUserId.HasValue)
            {
                command += $"{strDelim}surgeonUserId={surgeonUserId}";
                strDelim = "&";
            }
            if (beginDate.HasValue)
            {
                var strBeginDate = beginDate.Value.ToString("yyyy-MM-dd");

                command += $"{strDelim}begDate={strBeginDate}";
                strDelim = "&";
            }
            if (endDate.HasValue)
            {
                var strEndDate = endDate.Value.ToString("yyyy-MM-dd");

                command += $"{strDelim}endDate={strEndDate}";
                strDelim = "&";
            }

            var response = await WebUtility.WebRequest<List<SurgerySearchResult>>(command, HttpMethod.Get);

            return response;
        }

        public static async Task<int?> CreateSurgery(SurgeryPost surgery)
        {
            var command = $"api/surgery";

            var response = await WebUtility.PostBodyRequest<int>(command, surgery);

            return response;
        }

        public static async Task<Dictionary<int, Patient>> GetSurgeryPatients(List<Surgery> surgeries)
        {
            var result = new Dictionary<int, Patient>();

            if (surgeries == null || surgeries.Count == 0)
                return result;

            var patientIds = surgeries.GroupBy(s => s.PatientID).Select(g => g.Key).ToList();
            var patients = await PatientUtil.GetPatients(patientIds);

            foreach (var surgery in surgeries)
            {
                result[surgery.SurgeryID] = patients.FirstOrDefault(p => p.PatientID == surgery.PatientID);
            }

            return result;
        }

        public static async Task<string> AssignCard(int surgeryId, int cardId)
        {
            var command = string.Format("api/surgery/assignCard?surgeryId={0}&cardId={1}", surgeryId, cardId);

            var response = await WebUtility.PostBodyRequest<string>(command, 0);

            return response;
        }

        public static async Task<string> AssignFlow(int surgeryId, int flowId)
        {
            var command = string.Format("api/surgery/assignFlow?surgeryId={0}&flowId={1}", surgeryId, flowId);

            var response = await WebUtility.PostBodyRequest<string>(command, 0);

            return response;
        }

        public static async Task<string> AssignRoomSetup(int surgeryId, int roomSetupId)
        {
            var command = string.Format("api/surgery/assignRoomSetup?surgeryId={0}&roomSetupId={1}", surgeryId, roomSetupId);

            var response = await WebUtility.PostBodyRequest<string>(command, 0);

            return response;
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
