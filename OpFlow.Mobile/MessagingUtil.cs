using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public class MessagingUtil
    {
        public static async Task<List<Messaging>> GetMessages(int? caseId, int? caseGroupId, int? recipientId)
        {
            var providerId = AppSettings.CurrentUser.ProviderID;
            var locationId = AppSettings.CurrentUser.LocationID;

            var command = string.Format("api/message/list?providerId={0}&locationId={1}&caseId={2}&caseGroupId={3}&recipientId={4}", 
                providerId, locationId, caseId, caseGroupId, recipientId);
            var response = await WebUtility.WebRequest<List<Messaging>>(command, HttpMethod.Get);

            return response.OrderBy(r => r.InsertTimestamp).ToList();
        }

        public static async Task SendMessage(string message)
        {
            var providerId = AppSettings.CurrentUser.ProviderID;
            var locationId = AppSettings.CurrentUser.LocationID;

            //var command = string.Format("api/message/send?caseId={0}&providerId={1}&locationId={2}", caseId, providerId, locationId);
            //var response = await WebUtility.WebRequest<List<Messaging>>(command, HttpMethod.Get);

            //return response.OrderBy(r => r.MessageID).ToList();
        }

        public static async Task<List<MessagingGroup>> GetMessageGroups()
        {
            var providerId = AppSettings.CurrentUser.ProviderID;
            var locationId = AppSettings.CurrentUser.LocationID;

            var command = string.Format("api/message/groups?providerId={0}&locationId={1}", providerId, locationId);
            var response = await WebUtility.WebRequest<List<MessagingGroup>>(command, HttpMethod.Get);

            return response.OrderBy(r => r.CommunicationTargetName).ToList();
        }
    }
}
