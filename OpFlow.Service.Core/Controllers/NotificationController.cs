using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using OpFlow.Data;
using OpFlow.Service.DataAccess;
using OpFlow.Service.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace OpFlow.Service.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class NotificationController : OpFlowController
    {
        public NotificationController(
            IHttpContextAccessor httpContext, IMemoryCache memoryCache, SqlHelper sqlHelper) : base(httpContext, memoryCache, sqlHelper)
        {
        }

        //[Route("api/notification/device")]
        //[HttpPost]
        //public async Task<ActionResult> PostDevice(string handle, string platform)
        //{
        //    if (handle == null)
        //        return Request.CreateResponse(HttpStatusCode.Ambiguous);

        //    handle = handle.Replace(" ", "").ToUpper();

        //    var deviceId = await PushNotification.PostDevice(handle);

        //    try
        //    {
        //        var username = HttpContext.Current.User.Identity.Name;

        //        await PushNotification.PutDevice(deviceId,
        //            username,
        //            platform,
        //            handle,
        //            new string[] {});
        //    }
        //    catch (MessagingException e)
        //    {
        //        ReturnGoneIfHubResponseIsGone(e);
        //    }

        //    return Request.CreateResponse(HttpStatusCode.OK);
        //}

        //private static void ReturnGoneIfHubResponseIsGone(MessagingException e)
        //{
        //    var webex = e.InnerException as WebException;
        //    if (webex.Status == WebExceptionStatus.ProtocolError)
        //    {
        //        var response = (HttpWebResponse)webex.Response;
        //        if (response.StatusCode == HttpStatusCode.Gone)
        //            throw new HttpRequestException(HttpStatusCode.Gone.ToString());
        //    }
        //}

        //[Route("api/notification")]
        //[HttpPost]
        //public async Task<ActionResult> PostNotification(string pns, [FromBody]NotificationMessage message)
        //{
        //    var sendingUser = HttpContext.Current.User.Identity.Name;
        //    var ret = HttpStatusCode.InternalServerError;

        //    var user = await GetUserSecurity();
            

        //    var sender = await _sqlHelper.GetUser(user.SelectedLocation, user.UserID);

        //    var senderName = $"{sender.LastName}, {sender.FirstName}";
        //    var outcome = await PushNotification.PostNotification(sendingUser, senderName, message.UserName, message.Message);

        //    if (outcome != null)
        //    {
        //        if (!((outcome.State == NotificationOutcomeState.Abandoned) ||
        //              (outcome.State == NotificationOutcomeState.Unknown)))
        //        {
        //            ret = HttpStatusCode.OK;
        //        }
        //    }

        //    return Request.CreateResponse(ret);
        //}

        //[Route("api/notification/broadcast")]
        //[HttpPost]
        //public async Task<ActionResult> BroadcastNotification([FromBody]NotificationItem item)
        //{
        //    await PushNotification.BroadcastNotification(item.Message);

        //    return Ok();
        //}
    }
}