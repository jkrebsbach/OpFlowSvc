using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public abstract class WebUtility
    {
        private static HttpClient _client;
        private static string _rootUrl = "https://opflowservice.azurewebsites.net/api";

        static WebUtility()
        {
            _client = new HttpClient();
            _client.MaxResponseContentBufferSize = 256000;
        }

        public static async Task<List<Schedule>> GetSchedules()
        {
            var response = await WebRequest("schedule");

            var result = JsonConvert.DeserializeObject<List<Schedule>>(response);

            return result;
        }

        public static async Task<Schedule> GetSchedule(int scheduleId)
        {
            var response = await WebRequest(string.Format("schedule/{0}", scheduleId));

            var result = JsonConvert.DeserializeObject<Schedule>(response);

            return result;
        }

        private static async Task<string> WebRequest(string command)
        {
            var result = string.Empty;

            var uri = Path.Combine(_rootUrl, command);

            using (var response = await _client.GetAsync(uri))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);

                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }
    }
}
