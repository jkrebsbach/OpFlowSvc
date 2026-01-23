using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpFlow.Service.Core.DataAccess
{
    public class AMAHelper
    {
        private string _clientId;
        private string _clientSecret;
        private string _jwtToken;

        public AMAHelper(IConfiguration configuration) 
        { 
            _clientId = configuration.GetValue<string>("AMA:Key") ?? "";
            _clientSecret = configuration.GetValue<string>("AMA:Secret") ?? "";
        }

        public async Task GetAMAToken()
        {
            var httpClient = new HttpClient();

            var credentials = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"{_clientId}:{_clientSecret}"));
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Basic {credentials}");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            });

            try
            {
                var response = await httpClient.PostAsync("https://api-platform.ama-assn.org/token", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();

                    var authToken = JsonSerializer.Deserialize<AuthToken>(result);

                    _jwtToken = authToken?.AccessToken ?? "";
                }
                else
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception(stringResponse);
                }

            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }

        public async Task<List<ReleaseFile>> GetReleases()
        {
            if (string.IsNullOrEmpty(_jwtToken))
                await GetAMAToken();

            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_jwtToken}");

            try
            {
                var response = await httpClient.GetAsync($"https://api-platform.ama-assn.org/cpt-zip/1.0.0/releases");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var releases = JsonSerializer.Deserialize<List<ReleaseFile>>(content);

                    if (releases == null) throw new Exception("Failed to parse response: " + content);
                    return releases;
                }
                else
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception(stringResponse);
                }

            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }

        public async Task<byte[]> GetCPT()
        {
            if (string.IsNullOrEmpty(_jwtToken))
                await GetAMAToken();

            var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_jwtToken}");
            httpClient.DefaultRequestHeaders.Add("accept", $"application/zip");

            try
            {
                var releases = await GetReleases();
                var release = releases.OrderByDescending(r => r.DateValue).First().Id;

                var response = await httpClient.GetAsync($"https://api-platform.ama-assn.org/cpt-zip/1.0.0/files?release={release}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsByteArrayAsync();

                    return content;
                }
                else
                {
                    var stringResponse = await response.Content.ReadAsStringAsync();
                    throw new Exception(stringResponse);
                }

            }
            catch (Exception ex)
            {
                var error = ex.Message;
                throw;
            }
        }

        public class ReleaseFile
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }
            [JsonPropertyName("date")]
            public string Date { get; set; }

            [JsonIgnore]
            public DateTime DateValue => DateTime.Parse(Date);
        }
    }
}
