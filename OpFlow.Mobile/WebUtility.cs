using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
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

        public static void SetEnvironment(bool productionEnvironment)
        {
            if (productionEnvironment)
                _rootUrl = "https://opflowsvc.azurewebsites.net";
        }

        static WebUtility()
        {
            _client = new HttpClient();

            _client.BaseAddress = new Uri(_rootUrl);
            _client.MaxResponseContentBufferSize = 256000;
        }

        public static String GetWebUrl(string command)
		{
			return Path.Combine(_rootUrl, command);
		}

        internal static async Task<T> WebRequest<T>(string command, HttpMethod verb, List<KeyValuePair<string, string>> formData = null)
        {
            var result = string.Empty;

            if (!AppSettings.UserAuthenticated)
            {
                throw new AuthenticationException("No authenticated user");
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

        internal static async Task<T> SendBodyRequest<T>(string command, object bodyData, 
                         HttpMethod httpMethod)
        {
            var json = JsonConvert.SerializeObject(bodyData);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            return await TransmitValues<T>(command, content, httpMethod);
        }

        private static async Task<T> TransmitValues<T>(string command, HttpContent content, HttpMethod httpMethod)
        {
            try
            {
                var request = new HttpRequestMessage(httpMethod, command)
                    { Content = content };

                using (var response = await _client.SendAsync(request))
                {
                    if (response.StatusCode != HttpStatusCode.OK &&
                        response.StatusCode != HttpStatusCode.Created)
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
                var authToken = await TransmitValues<AuthToken>("/Token", content, HttpMethod.Post);

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
