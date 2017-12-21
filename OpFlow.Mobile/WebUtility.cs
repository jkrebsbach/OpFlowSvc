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

        static WebUtility()
        {
            _client = new HttpClient();
            _client.MaxResponseContentBufferSize = 256000;
        }

        public static async Task<List<Schedule>> GetSchedule()
        {
            var response = await WebRequest();

            var result = JsonConvert.DeserializeObject<List<Schedule>>(response);

            return result;
        }

        private static async Task<string> WebRequest()
        {
            var result = string.Empty;

            var rxcui = "198440";
            var uri = new Uri(string.Format(@"http://rxnav.nlm.nih.gov/REST/RxTerms/rxcui/{0}/allinfo", rxcui));

            using (var response = await _client.GetAsync(uri))
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    Console.Out.WriteLine("Error fetching data. Server returned status code: {0}", response.StatusCode);

                var content = await response.Content.ReadAsStringAsync();
            }

            return result;
        }
    }
}
