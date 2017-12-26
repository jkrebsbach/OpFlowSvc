using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpFlow.Data;

namespace OpFlow.Mobile
{
    public abstract class WebUtility
    {
        private static HttpClient _client;
        private static string _rootUrl = "https://opflowservice.azurewebsites.net";

        private static AuthToken _authToken;

        public static bool UserAuthenticated => _authToken != null;

        static WebUtility()
        {
            _client = new HttpClient();

            _client.BaseAddress = new Uri(_rootUrl);
            _client.MaxResponseContentBufferSize = 256000;
        }

        public static async Task<List<Schedule>> GetSchedules(DateTime scheduleDate)
        {
            var response = await WebRequest("api/schedule");

            var result = JsonConvert.DeserializeObject<List<Schedule>>(response);

            return result;
        }

        public static async Task<Schedule> GetSchedule(int scheduleId)
        {
            var response = await WebRequest(string.Format("api/schedule/{0}", scheduleId));

            var result = JsonConvert.DeserializeObject<Schedule>(response);

            return result;
        }

        private static async Task<string> WebRequest(string command)
        {
            var result = string.Empty;

            var uri = Path.Combine(_rootUrl, command);

            if (_authToken == null || _authToken.ExpiresDate < DateTime.Now)
            {
                throw new Exception("No authenticated user");
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken?.AccessToken);
            using (var response = await _client.GetAsync(uri))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);

                result = await response.Content.ReadAsStringAsync();
            }

            return result;
        }

        public static async Task LoginUser(string username, string password)
        {
            var formData = new List<KeyValuePair<string, string>>();
            formData.Add(new KeyValuePair<string, string>("grant_type", "password"));
            formData.Add(new KeyValuePair<string, string>("username", "OpFlow@OpFlow.com"));
            formData.Add(new KeyValuePair<string, string>("password", "OpFlow1!"));

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "/Token");
                request.Content = new FormUrlEncodedContent(formData);

                using (var response = await _client.SendAsync(request))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);

                    var responseString = await response.Content.ReadAsStringAsync();

                    _authToken = JsonConvert.DeserializeObject<AuthToken>(responseString);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
