using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public static class MessagingUtil
    {
        public static async Task<List<Messaging>> GetMessages(int? surgeryId, int? caseGroupId, int? recipientId)
        {
            var command = string.Format("api/message/list?surgeryId={0}&caseGroupId={1}&recipientId={2}", 
                surgeryId, caseGroupId, recipientId);
            var response = await WebUtility.WebRequest<List<Messaging>>(command, HttpMethod.Get);

            return response.OrderBy(r => r.InsertTimestamp).ToList();
        }

        public static async Task<MessagingGroup> CreateGroup(int surgeryId, List<int> userIds)
        {
            var providerId = AppSettings.CurrentUser.ProviderID;
            var locationId = AppSettings.CurrentUser.LocationID;

            //var command = string.Format("api/message/send?caseId={0}&providerId={1}&locationId={2}", caseId, providerId, locationId);
            //var response = await WebUtility.WebRequest<List<Messaging>>(command, HttpMethod.Get);

            //return response.OrderBy(r => r.MessageID).ToList();

            return new MessagingGroup()
            {
                SurgeryID = surgeryId,
                CaseGroupID = 1
            };
        }

        public static async Task SendMessage(MessagingGroup messagingGroup, string message)
        {
            var command = string.Format("api/message?surgeryId={0}&communicationUserID={1}",
                messagingGroup.SurgeryID, messagingGroup.CommunicationUserID);

            var messagePost = new MessagePost()
            {
                Message = message
            };
            var response = await WebUtility.SendBodyRequest<int>(command, messagePost, HttpMethod.Put);
        }

        public static async Task<List<MessagingGroup>> GetMessageGroups(bool onlyToday)
        {
            var command = string.Format("api/message/groups?startDate={0}",
                                       DateTime.Today.ToString("M/d/yyyy"));

            if (onlyToday)
            {
                command += string.Format("&endDate={0}", DateTime.Today.ToString("M/d/yyyy"));
            }

            var response = await WebUtility.WebRequest<List<MessagingGroup>>(command, HttpMethod.Get);

            return response.ToList();
        }
    }
}
