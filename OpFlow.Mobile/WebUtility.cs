using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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
                    return default(T);

                result = await response.Content.ReadAsStringAsync();
            }

            return JsonConvert.DeserializeObject<T>(result);
        }

        internal static async Task<T> PostBodyRequest<T>(string command, object bodyData)
        {
            var json = JsonConvert.SerializeObject(bodyData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await PostValues<T>(command, content);
        }

        private static async Task<T> PostValues<T>(string command, HttpContent content)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, command)
                    { Content = content };

                using (var response = await _client.SendAsync(request))
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);

                    var responseString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(responseString);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        internal static async Task<AuthToken> LoginUser(string username, string password)
        {
            var formDataCollection = new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>( "grant_type", "password"),
                new KeyValuePair<string, string>(  "username", username),
                new KeyValuePair<string, string>("password", password)
            };

            var content = new FormUrlEncodedContent(formDataCollection);

            try
            {
                var authToken = await PostValues<AuthToken>("/Token", content);

                if (authToken.AccessToken != null)
                {
                    _client.DefaultRequestHeaders.Authorization =
                        new AuthenticationHeaderValue("Bearer", authToken?.AccessToken);

                    return authToken;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

            return null;
        }
    }
}
