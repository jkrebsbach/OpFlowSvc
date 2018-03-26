using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Web;

namespace OpFlow.Service.Models
{
    public abstract class NotificationSystem
    {
        public static string NotifyUser(string cellPhone)
        {
            string result = null;

            var numbers = new List<string>()
            {
                cellPhone
            };

            using (var client = new WebClient())
            {
                numbers.ForEach(num => result = SendMessage(client, num));
            }

            return result;
        }

        private static string SendMessage(WebClient client, string phoneNumber)
        {
            if (string.IsNullOrEmpty(phoneNumber))
                return $"INVALID PHONENUMBER - {phoneNumber}";

            if (phoneNumber.Length < 11)
            {
                phoneNumber = $"1{phoneNumber}";
            }

            if (phoneNumber.Length != 11)
            {
                return $"INVALID PHONENUMBER - {phoneNumber} - Expected 11 digit numeric string 15555555";
            }

            var apiUrl = ConfigurationManager.AppSettings["NexmoApiUrl"];
            var apiKey = ConfigurationManager.AppSettings["NexmoApiKey"];
            var apiSecret = ConfigurationManager.AppSettings["NexmoApiSecret"];

            var payload = $"Surgery schedule modified - please review schedule";

            var urlTarget = $"{apiUrl}?api_key={apiKey}&api_secret={apiSecret}&to={phoneNumber}&from=12016728961&text={payload}";

            System.Threading.Thread.Sleep(1000);
            return client.DownloadString(urlTarget);
        }
    }
}