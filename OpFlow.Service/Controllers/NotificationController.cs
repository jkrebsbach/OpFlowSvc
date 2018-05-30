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
        public async Task<string> PostDevice(string handle = null)
        {
            return await PushNotification.PostDevice(handle);
        }



        // PUT api/notification/device?id=abc
        // This creates or updates a registration (with provided channelURI) at the specified id
        [SwaggerOperation("RegisterDevice")]
        [Route("api/notification/device")]
        [SwaggerResponse(HttpStatusCode.OK)]
        [HttpPut]
        public async Task<HttpResponseMessage> PutDevice(string id, NotificationDeviceRegistration deviceUpdate)
        {
            try
            {
                await PushNotification.PutDevice(id,
                    HttpContext.Current.User.Identity.Name,
                    deviceUpdate.Platform,
                    deviceUpdate.Handle,
                    deviceUpdate.Tags);

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

            var outcome = await PushNotification.PostNotification(sendingUser, message.UserName, message.Message);

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
            PushNotification.BroadcastNotification(item.Message);

            return Ok();
        }
    }
}