using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Azure.NotificationHubs;

namespace OpFlow.Service.Models
{
    public abstract class PushNotification
    {
        private static NotificationHubClient _hub;

        private static NotificationHubClient Hub
        {
            get
            {
                if (_hub == null)
                {
                    var connectionString = ConfigurationManager.ConnectionStrings["PushNotificationConnection"].ConnectionString;

                    var notificationHubPath = ConfigurationManager.AppSettings["PushNotificationHub"];
                    // Create a new Notification Hub client.
                    _hub = NotificationHubClient.CreateClientFromConnectionString(connectionString, notificationHubPath);
                }

                return _hub;
            }
        }

        /// <summary>
        /// register device with Push Notification server
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        public static async Task<string> PostDevice(string handle)
        {
            string newRegistrationId = null;

            // make sure there are no existing registrations for this push handle (used for iOS and Android)
            if (handle != null)
            {
                var registrations = await Hub.GetRegistrationsByChannelAsync(handle, 100);

                foreach (var registration in registrations)
                {
                    if (newRegistrationId == null)
                    {
                        newRegistrationId = registration.RegistrationId;
                    }
                    else
                    {
                        await Hub.DeleteRegistrationAsync(registration);
                    }
                }
            }

            return newRegistrationId ?? await Hub.CreateRegistrationIdAsync();
        }

        /// <summary>
        /// Associate username to registered device
        /// </summary>
        /// <param name="id"></param>
        /// <param name="username"></param>
        /// <param name="platform"></param>
        /// <param name="handle"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static async Task PutDevice(string id, 
            string username, string platform, string handle, string [] tags)
        {
            RegistrationDescription registration = null;
            switch (platform)
            {
                case "mpns":
                    registration = new MpnsRegistrationDescription(handle);
                    break;
                case "wns":
                    registration = new WindowsRegistrationDescription(handle);
                    break;
                case "apns":
                    registration = new AppleRegistrationDescription(handle);
                    break;
                case "gcm":
                    registration = new GcmRegistrationDescription(handle);
                    break;
                default:
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
            }

            registration.RegistrationId = id;
            
            // add check if user is allowed to add these tags
            registration.Tags = new HashSet<string>(tags ?? new string[] { });
            registration.Tags.Add("username:" + username);

            await Hub.CreateOrUpdateRegistrationAsync(registration);
        }

        /// <summary>
        /// Send notification to specific user 
        /// </summary>
        /// <param name="sendingUser"></param>
        /// <param name="targetUser"></param>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task<NotificationOutcome> PostNotification(
            string sendingUser, string targetUser, string message)
        {
            string[] userTag = new string[2];
            userTag[0] = "username:" + targetUser;
            userTag[1] = "from:" + sendingUser;

            NotificationOutcome outcome = null;

            // What systems are currently implemented?
            var pnsSystems = new [] {"apns"};

            foreach (var pns in pnsSystems)
            {
                switch (pns.ToLower())
                {
                    case "wns":
                        // Windows 8.1 / Windows Phone 8.1
                        var toast = @"<toast><visual><binding template=""ToastText01""><text id=""1"">" +
                                    "From " + sendingUser + ": " + message + "</text></binding></visual></toast>";
                        outcome = await Hub.SendWindowsNativeNotificationAsync(toast, userTag);
                        break;
                    case "apns":
                        // iOS
                        var alert = $"{{\"aps\":{{\"alert\":\"From {sendingUser}: {message}\", \"sound\":\"default\"}} }}";
                        //var alert = "{\"aps\":{\"alert\":\"" + "From " + sendingUser + ": " + message + "\"}}";
                        outcome = await Hub.SendAppleNativeNotificationAsync(alert, userTag);
                        break;
                    case "gcm":
                        // Android
                        var notif = "{ \"data\" : {\"message\":\"" + "From " + sendingUser + ": " + message + "\"}}";
                        outcome = await Hub.SendGcmNativeNotificationAsync(notif, userTag);
                        break;
                }
            }

            return outcome;
        }

        /// <summary>
        /// Broadcast message to all subscribing users
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static async Task BroadcastNotification(string message)
        {
            try
            {
                // iOS payload
                var appleNotificationPayload = "{\"aps\":{\"alert\":\"" + message + "\"}}";
                var result = await Hub.SendAppleNativeNotificationAsync(appleNotificationPayload);

                // Android payload
                var androidNotificationPayload = "{ \"data\" : {\"message\":\"" + message + "\"}}";
                result = await Hub.SendGcmNativeNotificationAsync(androidNotificationPayload);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}