using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Tracing;
using Microsoft.Azure.NotificationHubs;
using OpFlow.Data;

namespace OpFlow.Service.Controllers
{
    [Authorize]
    public class NotificationController : ApiController
    {
        public async Task<IHttpActionResult> Post([FromBody]NotificationItem item)
        {
            // Is this important?...
            //TodoItem current = await InsertAsync(item);
            var current = item;

            // Get the settings for the server project.
            HttpConfiguration config = this.Configuration;

            //MobileAppSettingsDictionary settings =
            //    this.Configuration.GetMobileAppSettingsProvider().GetMobileAppSettings();

            //// Get the Notification Hubs credentials for the Mobile App.
            //string notificationHubName = settings.NotificationHubName;
            //string notificationHubConnection = settings
            //    .Connections[MobileAppSettingsKeys.NotificationHubConnectionString].ConnectionString;

            //// Create a new Notification Hub client.
            //NotificationHubClient hub = NotificationHubClient
            //    .CreateClientFromConnectionString(notificationHubConnection, notificationHubName);


            var connectionString = "Endpoint=sb://opflow.servicebus.windows.net/;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=FvZ3wmulqHDTy+22bTThgjWi0H9kI+SXl/5on8fNL5A=";
            var notificationHubPath = "opflow";
            // Create a new Notification Hub client.
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString(connectionString, notificationHubPath);

            // iOS payload
            var appleNotificationPayload = "{\"aps\":{\"alert\":\"" + item.Message + "\"}}";

            try
            {
                // Send the push notification and log the results.
                var result = await hub.SendAppleNativeNotificationAsync(appleNotificationPayload);

                // Write the success result to the logs.
                //config.Services.GetTraceWriter().Info(result.State.ToString());

                var success = result.State.ToString();
            }
            catch (System.Exception ex)
            {
                var tmpInt = 0;
                // Write the failure result to the logs.
                //config.Services.GetTraceWriter()
                //.Error(ex.Message, null, "Push.SendAsync Error");
            }
            return CreatedAtRoute("Tables", new { id = current.Id }, current);
        }
    }
}