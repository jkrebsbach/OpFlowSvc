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

        static WebUtility()
        {
            _client = new HttpClient();

            _client.BaseAddress = new Uri(_rootUrl);
            _client.MaxResponseContentBufferSize = 256000;
        }

        internal static async Task<T> WebRequest<T>(string command, HttpMethod verb, List<KeyValuePair<string, string>> formData = null)
        {
            var result = string.Empty;

            if (!AppSettings.UserAuthenticated)
            {
                throw new Exception("No authenticated user");
            }
            var request = new HttpRequestMessage(verb, command);
            if (formData != null)
                request.Content = new FormUrlEncodedContent(formData);

            using (var response = await _client.SendAsync(request))
            {
                if (response.StatusCode == HttpStatusCode.NotFound)
                    return default(T);

                if (response.StatusCode != HttpStatusCode.OK)
                    Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);

                result = await response.Content.ReadAsStringAsync();
            }

            return JsonConvert.DeserializeObject<T>(result);
        }

        internal static async Task<AuthToken> LoginUser(string username, string password)
        {
            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", username),
                new KeyValuePair<string, string>("password", password)
            };

            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, "/Token")
                    { Content = new FormUrlEncodedContent(formData)};

                using (var response = await _client.SendAsync(request))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);

                    var responseString = await response.Content.ReadAsStringAsync();

                    var authToken = JsonConvert.DeserializeObject<AuthToken>(responseString);
                    _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authToken?.AccessToken);

                    return authToken;
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
