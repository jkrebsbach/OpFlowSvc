using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Tracing;
using Microsoft.Azure.NotificationHubs;
using Microsoft.Azure.NotificationHubs.Messaging;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;
using Swashbuckle.Swagger.Annotations;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class NotificationController : ApiController
    {
        private NotificationHubClient _hub;

        [SwaggerOperation("RegisterDevice")]
        [Route("api/notification/device")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        public async Task<HttpResponseMessage> PostDevice(string handle, string platform)
        {
            if (handle == null)
                return Request.CreateResponse(HttpStatusCode.Ambiguous);

            handle = handle.Replace(" ", "").ToUpper();

            var deviceId = await PushNotification.PostDevice(handle);

            try
            {
                var username = HttpContext.Current.User.Identity.Name;

                await PushNotification.PutDevice(deviceId,
                    username,
                    platform,
                    handle,
                    new string[] {});
            }
            catch (MessagingException e)
            {
                ReturnGoneIfHubResponseIsGone(e);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }

        private static void ReturnGoneIfHubResponseIsGone(MessagingException e)
        {
            var webex = e.InnerException as WebException;
            if (webex.Status == WebExceptionStatus.ProtocolError)
            {
                var response = (HttpWebResponse)webex.Response;
                if (response.StatusCode == HttpStatusCode.Gone)
                    throw new HttpRequestException(HttpStatusCode.Gone.ToString());
            }
        }

        [SwaggerOperation("SendNotification")]
        [Route("api/notification")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        public async Task<HttpResponseMessage> PostNotification(string pns, [FromBody]NotificationMessage message)
        {
            var sendingUser = HttpContext.Current.User.Identity.Name;
            var ret = HttpStatusCode.InternalServerError;

            var user = await CacheUtil.GetUserSecurity();
            var sqlHelper = new SqlHelper(user.CaseDatabaseName);

            var sender = await sqlHelper.GetUser(user.ProviderID, user.LocationID, user.UserID);

            var senderName = $"{sender.LastName}, {sender.FirstName}";
            var outcome = await PushNotification.PostNotification(sendingUser, senderName, message.UserName, message.Message);

            if (outcome != null)
            {
                if (!((outcome.State == NotificationOutcomeState.Abandoned) ||
                      (outcome.State == NotificationOutcomeState.Unknown)))
                {
                    ret = HttpStatusCode.OK;
                }
            }

            return Request.CreateResponse(ret);
        }

        [SwaggerOperation("BroadcastNotification")]
        [Route("api/notification/broadcast")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPost]
        public async Task<IHttpActionResult> BroadcastNotification([FromBody]NotificationItem item)
        {
            await PushNotification.BroadcastNotification(item.Message);

            return Ok();
        }
    }
}